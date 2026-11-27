
using Application.Common.Models;
using Application.Repository.Services;
using System.Linq;

namespace Application.Core.OrgLocation_core.Queries;

public record SimpleOrgLocationWithPaginationQuery : PageFilter<OrgLocation, SimpleOrgLocationWithPaginationDTO>
{
}

public record SimpleOrgLocationWithPaginationDTO : IMapFrom<OrgLocation>
{


    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("آدرس")]
    public string? Address { get; init; }

    [PropConfig("تلفن")]
    public string? Tell { get; init; }

    [PropConfig("فکس")]
    public string? Fax { get; init; }

    [PropConfig("ایمیل")]
    public string? Email { get; init; }

    [PropConfig("آدرس وبسایت")]
    public string? Url { get; init; }

    [PropConfig("کلید")] 
    public Guid GUID { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocation, SimpleOrgLocationWithPaginationDTO>();
    }
}

public class
    SimpleOrgLocationWithPaginationQueryHandler : IRequestHandler<SimpleOrgLocationWithPaginationQuery,
    PaginatedList<SimpleOrgLocationWithPaginationDTO>>
{
    private OrgLocationService OrgLocationService;
    private UserRoleService UserRoleService;
    public SimpleOrgLocationWithPaginationQueryHandler(OrgLocationService OrgLocationService, UserRoleService userRoleService)
    {
        this.OrgLocationService = OrgLocationService;
        this.UserRoleService = userRoleService;
    }

    public async Task<PaginatedList<SimpleOrgLocationWithPaginationDTO>> Handle(SimpleOrgLocationWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var userRoleInfo = await UserRoleService.GetUserRoleInfo();

        IQueryable<OrgLocation> queryable = OrgLocationService.Table
            .Where(a => a.Path.StartsWith(userRoleInfo.OrgLocationPath));

        return await request.ToPaging(queryable, OrgLocationService.mapper);
    }
}

