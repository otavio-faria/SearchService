using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SearchService.Domain.Entities;
using SearchService.Domain.Interfaces;
using SearchService.Domain.Models;
using SearchService.Infrastructure.Configurations;

namespace SearchService.Infrastructure.Persistence;

public class MongoSearchRepository : ISearchRepository
{
    private readonly IMongoCollection<MenuItem> _menuItems;

    public MongoSearchRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _menuItems = database.GetCollection<MenuItem>(mongoDbSettings.Value.MenuItemsCollectionName);
        
        CreateIndexes();
    }

    public async Task<SearchResult> SearchMenuItemsAsync(SearchCriteria criteria)
    {
        var filterBuilder = Builders<MenuItem>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(criteria.Name))
        {
            filter &= filterBuilder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(criteria.Name, "i"));
        }

        if (criteria.IsAvailable.HasValue)
        {
            filter &= filterBuilder.Eq(x => x.IsAvailable, criteria.IsAvailable.Value);
        }

        if (criteria.MinPrice.HasValue)
        {
            filter &= filterBuilder.Gte(x => x.Price, criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            filter &= filterBuilder.Lte(x => x.Price, criteria.MaxPrice.Value);
        }

        var totalCount = await _menuItems.CountDocumentsAsync(filter);

        var sortDefinition = GetSortDefinition(criteria.SortBy, criteria.SortDescending);

        var skip = (criteria.Page - 1) * criteria.PageSize;
        
        var items = await _menuItems
            .Find(filter)
            .Sort(sortDefinition)
            .Skip(skip)
            .Limit(criteria.PageSize)
            .ToListAsync();

        return new SearchResult
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = criteria.Page,
            PageSize = criteria.PageSize
        };
    }

    public async Task<List<MenuItem>> GetAllMenuItemsAsync()
    {
        return await _menuItems.Find(_ => true).ToListAsync();
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(string id)
    {
        return await _menuItems.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    private SortDefinition<MenuItem> GetSortDefinition(string? sortBy, bool sortDescending)
    {
        var sortBuilder = Builders<MenuItem>.Sort;
        
        return sortBy?.ToLower() switch
        {
            "name" => sortDescending ? sortBuilder.Descending(x => x.Name) : sortBuilder.Ascending(x => x.Name),
            "price" => sortDescending ? sortBuilder.Descending(x => x.Price) : sortBuilder.Ascending(x => x.Price),
            _ => sortBuilder.Ascending(x => x.Name)
        };
    }

    private void CreateIndexes()
    {
        var indexKeysDefinition = Builders<MenuItem>.IndexKeys
            .Ascending(x => x.Name)
            .Ascending(x => x.Price)
            .Ascending(x => x.IsAvailable);

        var indexModel = new CreateIndexModel<MenuItem>(indexKeysDefinition);
        _menuItems.Indexes.CreateOne(indexModel);

        var textIndexKeys = Builders<MenuItem>.IndexKeys
            .Text(x => x.Name)
            .Text(x => x.Description);
        
        var textIndexModel = new CreateIndexModel<MenuItem>(textIndexKeys);
        _menuItems.Indexes.CreateOne(textIndexModel);
    }
} 