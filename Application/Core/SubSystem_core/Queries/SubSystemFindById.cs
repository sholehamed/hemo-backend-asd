namespace Application.Core.SubSystem_core.Queries;

public record SubSystemFindByIdQuery(Guid GUID) : IRequest<SubSystemFindByIdDTO>;

public record SubSystemFindByIdDTO : IMapFrom<SubSystem>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("آدرس سرویس")] public string Host { get; init; }

    [PropConfig("پورت سرویس")] public int Port { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    [PropConfig("ساعت شروع فعالیت")] public TimeSpan? StartTime { get; set; }
    [PropConfig("ساعت پایان فعالیت")] public TimeSpan? EndTime { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SubSystem, SubSystemFindByIdDTO>();
    }
}

public class
    SubSystemFindByIdQueryHandler : IRequestHandler<SubSystemFindByIdQuery, SubSystemFindByIdDTO>
{
    private readonly SubSystemService SubSystemService;

    public SubSystemFindByIdQueryHandler(SubSystemService SubSystemService)
    {
        this.SubSystemService = SubSystemService;
    }

    public async Task<SubSystemFindByIdDTO> Handle(SubSystemFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        var Data = SubSystemService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<SubSystemFindByIdDTO>(SubSystemService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}