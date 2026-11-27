namespace Application.Core.Api_core.Queries;

public record ApiFindAllQuery : IRequest<List<ApiFindAllDTO>>
{
}

public record ApiFindAllDTO : IMapFrom<Api>
{
    [PropConfig("مسیر درخواست ارسالی")] public string DownstreamPathTemplate { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Api, ApiFindAllDTO>()
            .ForMember(dest=>dest.DownstreamPathTemplate,opt=>opt.MapFrom(src=>src.DownstreamPathTemplate+" "+src.APITypeName))
            ;
    }
}

public class
    ApiFindAllQueryHandler : IRequestHandler<ApiFindAllQuery,
    List<ApiFindAllDTO>>
{
    private readonly ApiService ApiService;

    public ApiFindAllQueryHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<List<ApiFindAllDTO>> Handle(ApiFindAllQuery request,
        CancellationToken cancellationToken)
    {
        return await ApiService.Table.AsNoTracking()
            .ProjectTo<ApiFindAllDTO>(ApiService.mapper.ConfigurationProvider).ToListAsync();
    }
}