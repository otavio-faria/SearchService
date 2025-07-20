using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Queries;
using SearchService.Domain.Models;

namespace SearchService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

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
        return Ok(result);
    }

    /// <summary>
    /// Obtém um item específico do cardápio por ID
    /// </summary>
    [HttpGet("menu-items/{id}")]
    public async Task<ActionResult> GetMenuItemById(string id)
    {
        var query = new GetMenuItemByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
} 