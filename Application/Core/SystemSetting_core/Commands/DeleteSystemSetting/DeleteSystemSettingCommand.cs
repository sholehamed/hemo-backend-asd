
namespace Application.Core.SystemSetting_core.Commands.DeleteSystemSetting;

public record DeleteSystemSettingCommand(Guid GUID) : IRequest;

public class DeleteSystemSettingCommandHandler : IRequestHandler<DeleteSystemSettingCommand>
{
    private SystemSettingService SystemSettingService;

    public DeleteSystemSettingCommandHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task Handle(DeleteSystemSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = await SystemSettingService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        SystemSettingService.Table.Remove(entity);

        await SystemSettingService.SaveChangesAsync(cancellationToken);
    }

}

