
using Application.Common.Models;

namespace Application.Core.RoleAccess_core.Queries;

public record RoleAccessWithPaginationQuery : PageFilter<RoleAccess, RoleAccessWithPaginationDTO>
{
    public Guid RoleGUID { get; set; }
}

public record RoleAccessWithPaginationDTO : IMapFrom<RoleAccess>
{

    [PropConfig("منو")]
    public string MenuName{ get; init; }

    [PropConfig("نقش")]
    public string RoleName { get; init; }

    [PropConfig("سرویس")]
    public string ApiName { get; init; }
    [PropConfig("ادرس سرویس")]
    public string ApiAddress { get; init; }
    [PropConfig("متد سرویس")]
    public string ApiMethod { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccess, RoleAccessWithPaginationDTO>()
            .ForMember(dest=>dest.MenuName,opt=>opt.MapFrom(a=>a.Menu.label))
            .ForMember(dest=>dest.RoleName,opt=>opt.MapFrom(a=>a.Role.Name))
            .ForMember(dest=>dest.ApiName,opt=>opt.MapFrom(a=>(a.ApiGUID!=null)?a.Api.Title:"دسترسی کامل"))
            .ForMember(dest=>dest.ApiAddress,opt=>opt.MapFrom(a=>a.Api.UpstreamPathTemplate))
            .ForMember(dest=>dest.ApiMethod,opt=>opt.MapFrom(a=>a.Api.APITypeName))

            
            ;
        
    }
}

public class
    RoleAccessWithPaginationQueryHandler : IRequestHandler<RoleAccessWithPaginationQuery,
    PaginatedList<RoleAccessWithPaginationDTO>>
{
    private RoleAccessService RoleAccessService;
    private RoleService RoleService;
   
    public RoleAccessWithPaginationQueryHandler(RoleAccessService roleAccessService, RoleService roleService)
    {
        RoleAccessService = roleAccessService;
        RoleService = roleService;
    }

    public async Task<PaginatedList<RoleAccessWithPaginationDTO>> Handle(RoleAccessWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.RoleGUID);
        
        IQueryable<RoleAccess> queryable = RoleAccessService.Table.Where(A=>A.RoleGUID==request.RoleGUID);
        return await request.ToPaging( queryable,RoleAccessService.mapper);
    }
}

