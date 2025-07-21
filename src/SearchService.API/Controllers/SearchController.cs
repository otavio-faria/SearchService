using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Queries;
using SearchService.Domain.Models;
using Prometheus;
using Microsoft.AspNetCore.Authorization;

namespace SearchService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Cozinha, Cliente")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    
    // Métricas do Prometheus
    private static readonly Counter SearchRequestsTotal = Metrics
        .CreateCounter("search_requests_total", "Total number of search requests", new CounterConfiguration
        {
            LabelNames = new[] { "endpoint", "status" }
        });

    private static readonly Histogram SearchDuration = Metrics
        .CreateHistogram("search_duration_seconds", "Search request duration in seconds", new HistogramConfiguration
        {
            LabelNames = new[] { "endpoint" },
            Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
        });

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Busca itens do cardápio com filtros
    /// </summary>
    [HttpGet("menu-items")]
    public async Task<ActionResult<SearchResult>> SearchMenuItems(
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDescending = false)
    {
        using (SearchDuration.WithLabels("search-menu-items").NewTimer())
        {
            try
            {
                var query = new SearchMenuItemsQuery
                {
                    Name = name,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    IsAvailable = isAvailable,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100),
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _mediator.Send(query);
                SearchRequestsTotal.WithLabels("search-menu-items", "200").Inc();
                return Ok(result);
            }
            catch (Exception)
            {
                SearchRequestsTotal.WithLabels("search-menu-items", "500").Inc();
                throw;
            }
        }
    }

    /// <summary>
    /// Obtém um item específico do cardápio por ID
    /// </summary>
    [HttpGet("menu-items/{id}")]
    public async Task<ActionResult> GetMenuItemById(string id)
    {
        using (SearchDuration.WithLabels("get-menu-item-by-id").NewTimer())
        {
            try
            {
                var query = new GetMenuItemByIdQuery(id);
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    SearchRequestsTotal.WithLabels("get-menu-item-by-id", "404").Inc();
                    return NotFound();
                }
                
                SearchRequestsTotal.WithLabels("get-menu-item-by-id", "200").Inc();
                return Ok(result);
            }
            catch (Exception)
            {
                SearchRequestsTotal.WithLabels("get-menu-item-by-id", "500").Inc();
                throw;
            }
        }
    }
} 