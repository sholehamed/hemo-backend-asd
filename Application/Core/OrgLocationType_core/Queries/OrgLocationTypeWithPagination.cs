
using Application.Common.Models;

namespace Application.Core.OrgLocationType_core.Queries;

public record OrgLocationTypeWithPaginationQuery : PageFilter<OrgLocationType, OrgLocationTypeWithPaginationDTO>
{

}

public record OrgLocationTypeWithPaginationDTO : IMapFrom<OrgLocationType>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocationType, OrgLocationTypeWithPaginationDTO>();
    }
}

public class
    OrgLocationTypeWithPaginationQueryHandler : IRequestHandler<OrgLocationTypeWithPaginationQuery,
    PaginatedList<OrgLocationTypeWithPaginationDTO>>
{
    private OrgLocationTypeService OrgLocationTypeService;
    public OrgLocationTypeWithPaginationQueryHandler(OrgLocationTypeService OrgLocationTypeService)
    {
        this.OrgLocationTypeService = OrgLocationTypeService;
    }

    public async Task<PaginatedList<OrgLocationTypeWithPaginationDTO>> Handle(OrgLocationTypeWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<OrgLocationType> queryable = OrgLocationTypeService.Table;
        return await request.ToPaging( queryable,OrgLocationTypeService.mapper);
    }
}

