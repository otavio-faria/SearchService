using MediatR;
using SearchService.Application.Queries;
using SearchService.Domain.Entities;
using SearchService.Domain.Interfaces;

namespace SearchService.Application.Handlers;

public class GetMenuItemByIdHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItem?>
{
    private readonly ISearchRepository _searchRepository;

    public GetMenuItemByIdHandler(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public async Task<MenuItem?> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
    {
        return await _searchRepository.GetMenuItemByIdAsync(request.Id);
    }
} 