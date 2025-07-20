using System.Diagnostics.CodeAnalysis;

namespace SearchService.Domain.Entities;

[ExcludeFromCodeCoverage]
public class MenuItem(string name, string description, decimal price, bool isAvailable)
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public decimal Price { get; private set; } = price;
    public bool IsAvailable { get; private set; } = isAvailable;

    public void Update(string name, string description, decimal price, bool isAvailable)
    {
        Name = name;
        Description = description;
        Price = price;
        IsAvailable = isAvailable;
    }
} 