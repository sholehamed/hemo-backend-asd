
using Application.Common.Models;

namespace Application.Core.PasswordBlockList_core.Queries;

public record PasswordBlockListWithPaginationQuery : PageFilter<PasswordBlockList, PasswordBlockListWithPaginationDTO>
{

}

public record PasswordBlockListWithPaginationDTO : IMapFrom<PasswordBlockList>
{

    [PropConfig("پسورد")]
    public string Password { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    


    public void Mapping(Profile profile)
    {
        profile.CreateMap<PasswordBlockList, PasswordBlockListWithPaginationDTO>();
    }
}

public class
    PasswordBlockListWithPaginationQueryHandler : IRequestHandler<PasswordBlockListWithPaginationQuery,
    PaginatedList<PasswordBlockListWithPaginationDTO>>
{
    private PasswordBlockListService PasswordBlockListService;
    public PasswordBlockListWithPaginationQueryHandler(PasswordBlockListService PasswordBlockListService)
    {
        this.PasswordBlockListService = PasswordBlockListService;
    }

    public async Task<PaginatedList<PasswordBlockListWithPaginationDTO>> Handle(PasswordBlockListWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<PasswordBlockList> queryable = PasswordBlockListService.Table;
        return await request.ToPaging( queryable,PasswordBlockListService.mapper);
    }
}

