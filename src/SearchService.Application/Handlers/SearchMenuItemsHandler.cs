using MediatR;
using SearchService.Application.Queries;
using SearchService.Domain.Interfaces;
using SearchService.Domain.Models;

namespace SearchService.Application.Handlers;

public class SearchMenuItemsHandler : IRequestHandler<SearchMenuItemsQuery, SearchResult>
{
    private readonly ISearchRepository _searchRepository;

    public SearchMenuItemsHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public async Task<SearchResult> Handle(SearchMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var criteria = new SearchCriteria
        {
            Name = request.Name,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            IsAvailable = request.IsAvailable,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _searchRepository.SearchMenuItemsAsync(criteria);
    }
} 