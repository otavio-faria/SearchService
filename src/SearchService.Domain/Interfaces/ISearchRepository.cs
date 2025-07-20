using SearchService.Domain.Entities;
using SearchService.Domain.Models;

namespace SearchService.Domain.Interfaces;

public interface ISearchRepository
{
    Task<SearchResult> SearchMenuItemsAsync(SearchCriteria criteria);
    Task<List<MenuItem>> GetAllMenuItemsAsync();
    Task<MenuItem?> GetMenuItemByIdAsync(string id);
} 