using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Microsoft.AspNetCore.Authorization;

namespace SearchService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MetricsController : ControllerBase
    {
        private static readonly Counter SearchRequestsTotal = Metrics
            .CreateCounter("search_requests_total", "Total number of search requests", new CounterConfiguration
            {
                LabelNames = new[] { "method", "status" }
            });

        private static readonly Histogram SearchDuration = Metrics
            .CreateHistogram("search_duration_seconds", "Search request duration in seconds", new HistogramConfiguration
            {
                LabelNames = new[] { "method" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });

        private static readonly Gauge ActiveSearches = Metrics
            .CreateGauge("active_searches", "Number of active search operations");

        [HttpGet("search-stats")]
        public IActionResult GetSearchStats()
        {
            var averageDuration = SearchDuration.Count > 0 ? SearchDuration.Sum / SearchDuration.Count : 0;
            
            // Garantir que não há valores infinitos ou NaN
            if (double.IsInfinity(averageDuration) || double.IsNaN(averageDuration))
                averageDuration = 0;
                
            return Ok(new
            {
                TotalRequests = SearchRequestsTotal.Value,
                ActiveSearches = ActiveSearches.Value,
                AverageDuration = averageDuration
            });
        }

        [HttpPost("increment-search")]
        public IActionResult IncrementSearch([FromBody] SearchRequest request)
        {
            SearchRequestsTotal.WithLabels(request.Method, "200").Inc();
            ActiveSearches.Inc();
            
            using (SearchDuration.WithLabels(request.Method).NewTimer())
            {
                // Simular processamento
                Thread.Sleep(100);
            }
            
            ActiveSearches.Dec();
            
            return Ok(new { Message = "Search metric incremented" });
        }
    }

    public class SearchRequest
    {
        public string Method { get; set; } = "GET";
    }
} 