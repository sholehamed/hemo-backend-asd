
using Application.Common.Models;

namespace Application.Core.MenuApi_core.Queries;

public record MenuApiWithPaginationQuery : PageFilter<MenuApi, MenuApiWithPaginationDTO>
{

}

public record MenuApiWithPaginationDTO : IMapFrom<MenuApi>
{

    [PropConfig("منو")]
    public string MenuName { get; init; }

    [PropConfig("سرویس")]
    public string ApiName { get; init; }
    
    [PropConfig("آدرس سرویس")]
    public string ApiAddress { get; init; }
    [PropConfig("متد")]
    public string ApiMethod{ get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuApi, MenuApiWithPaginationDTO>()
            .ForMember(dest=>dest.MenuName,opt=>opt.MapFrom(a=>a.Menu.label))
            .ForMember(dest=>dest.ApiName,opt=>opt.MapFrom(a=>a.Api.Title))
            .ForMember(dest=>dest.ApiAddress,opt=>opt.MapFrom(a=>a.Api.UpstreamPathTemplate))
            .ForMember(dest=>dest.ApiMethod,opt=>opt.MapFrom(a=>a.Api.APITypeName))

            ;
    }
}

public class
    MenuApiWithPaginationQueryHandler : IRequestHandler<MenuApiWithPaginationQuery,
    PaginatedList<MenuApiWithPaginationDTO>>
{
    private MenuApiService MenuApiService;
    public MenuApiWithPaginationQueryHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task<PaginatedList<MenuApiWithPaginationDTO>> Handle(MenuApiWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<MenuApi> queryable = MenuApiService.Table;
        return await request.ToPaging( queryable,MenuApiService.mapper);
    }
}

