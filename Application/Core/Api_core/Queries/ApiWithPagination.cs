using Application.Common.Models;

namespace Application.Core.Api_core.Queries;

public record ApiWithPaginationQuery : PageFilter<Api, ApiWithPaginationDTO>
{
}

public record ApiWithPaginationDTO : IMapFrom<Api>
{

    [PropConfig("مسیر درخواست دریافتی")] public string UpstreamPathTemplate { get; init; }

    [PropConfig("سرویس از نوع عمومی می باشد")]
    public bool IsPublic { get; init; }
    [PropConfig("زیر سیستم")] public string SubSystemName { get; init; }

    [PropConfig("متد درخواست")] public string APITypeName { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    
    [PropConfig("عنوان")] public string Title { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Api, ApiWithPaginationDTO>()
            .ForMember(dest => dest.SubSystemName, opt => opt.MapFrom(src => src.SubSystem.Name));
    }
}

public class
    ApiWithPaginationQueryHandler : IRequestHandler<ApiWithPaginationQuery,
    PaginatedList<ApiWithPaginationDTO>>
{
    private readonly ApiService ApiService;

    public ApiWithPaginationQueryHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<PaginatedList<ApiWithPaginationDTO>> Handle(ApiWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Api> queryable = ApiService.Table;
        return await request.ToPaging(queryable, ApiService.mapper);
    }
}