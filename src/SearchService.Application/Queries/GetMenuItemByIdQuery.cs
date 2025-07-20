using MediatR;
using SearchService.Domain.Entities;

namespace SearchService.Application.Queries;

public class GetMenuItemByIdQuery : IRequest<MenuItem?>
{
    public string Id { get; set; } = string.Empty;
    
    public GetMenuItemByIdQuery(string id)
    {
        Id = id;
    }
} 