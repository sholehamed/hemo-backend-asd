namespace Application.Core.SubSystem_core.Commands.UpdateSubSystem;

public record UpdateSubSystemCommand : IRequest, IMapFrom<SubSystem>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("آدرس سرویس")] public string Host { get; init; }

    [PropConfig("پورت سرویس")] public int Port { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    [PropConfig("ساعت شروع فعالیت")] public TimeSpan? StartTime { get; set; }
    [PropConfig("ساعت پایان فعالیت")] public TimeSpan? EndTime { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateSubSystemCommand, SubSystem>();
    }
}

public class UpdateSubSystemCommandHandler : IRequestHandler<UpdateSubSystemCommand>
{
    private readonly SubSystemService SubSystemService;
    private ApiService ApiService;

    public UpdateSubSystemCommandHandler(SubSystemService subSystemService, ApiService apiService)
    {
        SubSystemService = subSystemService;
        ApiService = apiService;
    }
    public async Task Handle(UpdateSubSystemCommand request, CancellationToken cancellationToken)
    {
        await ApiService.TestHost(request.Host, request.Port);
        var OldEntity = await SubSystemService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        SubSystemService.mapper.Map(request, OldEntity);

        await SubSystemService.SaveChangesAsync(cancellationToken);

        ApiService.UpateApiListInBackground();
    }
}