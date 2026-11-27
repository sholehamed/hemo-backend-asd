namespace Application.Core.Api_core.Queries;

public record ApiLoadBySubsystemQuery(Guid SubSystemGUID) : IRequest<List<ApiLoadBySubsystemDTO>>
{
}

public record ApiLoadBySubsystemDTO : IMapFrom<Api>
{
    [PropConfig("کلید")] public Guid GUID { get; init; }

    [PropConfig("عنوان")] public string Name { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Api, ApiLoadBySubsystemDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title + " / " + src.DownstreamPathTemplate))
            ;
    }
}

public class
    ApiLoadBySubsystemQueryHandler : IRequestHandler<ApiLoadBySubsystemQuery,
    List<ApiLoadBySubsystemDTO>>
{
    private ApiService ApiService;

    public ApiLoadBySubsystemQueryHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<List<ApiLoadBySubsystemDTO>> Handle(ApiLoadBySubsystemQuery request,
        CancellationToken cancellationToken)
    {
        return await ApiService.Table.AsNoTracking().Where(a => a.SubSystemGUID == request.SubSystemGUID)
            .ProjectTo<ApiLoadBySubsystemDTO>(ApiService.mapper.ConfigurationProvider).ToListAsync();
    }
}