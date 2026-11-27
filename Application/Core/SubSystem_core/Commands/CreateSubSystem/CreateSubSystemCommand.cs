using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.SubSystem_core.Commands.CreateSubSystem;

public record CreateSubSystemCommand : IRequest<Guid>, IMapFrom<SubSystem>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("آدرس سرویس")] public string Host { get; init; }

    [PropConfig("پورت سرویس")] public int Port { get; init; }
 
    [PropConfig("ساعت شروع فعالیت")] public TimeSpan? StartTime { get; set; }
    [PropConfig("ساعت پایان فعالیت")] public TimeSpan? EndTime { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateSubSystemCommand, SubSystem>();
    }
}

public class CreateSubSystemCommandHandler : IRequestHandler<CreateSubSystemCommand, Guid>
{
    private readonly SubSystemService SubSystemService;

    private ApiService ApiService;

    public CreateSubSystemCommandHandler(SubSystemService subSystemService, ApiService apiService)
    {
        SubSystemService = subSystemService;
        ApiService = apiService;
    }

    public async Task<Guid> Handle(CreateSubSystemCommand request, CancellationToken cancellationToken)
    {
        await ApiService.TestHost(request.Host, request.Port);
        
        var entity = SubSystemService.mapper.Map<SubSystem>(request);
        entity.Active = true;
        SubSystemService.Table.Add(entity);

        await SubSystemService.SaveChangesAsync(cancellationToken);

        ApiService.UpateApiListInBackground();

        return entity.GUID;
    }
}