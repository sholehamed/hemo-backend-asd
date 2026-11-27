
using Application.Common.Models;

namespace Application.Core.OrgType_core.Queries;

public record OrgTypeWithPaginationQuery : PageFilter<OrgType, OrgTypeWithPaginationDTO>
{

}

public record OrgTypeWithPaginationDTO : IMapFrom<OrgType>
{

    [PropConfig("نوع شرکت")]
    public Guid OrgLocationTypeGUID { get; init; }

    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgType, OrgTypeWithPaginationDTO>();
    }
}

public class
    OrgTypeWithPaginationQueryHandler : IRequestHandler<OrgTypeWithPaginationQuery,
    PaginatedList<OrgTypeWithPaginationDTO>>
{
    private OrgTypeService OrgTypeService;
    public OrgTypeWithPaginationQueryHandler(OrgTypeService OrgTypeService)
    {
        this.OrgTypeService = OrgTypeService;
    }

    public async Task<PaginatedList<OrgTypeWithPaginationDTO>> Handle(OrgTypeWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<OrgType> queryable = OrgTypeService.Table;
        return await request.ToPaging( queryable,OrgTypeService.mapper);
    }
}

