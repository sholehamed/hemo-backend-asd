namespace Application.Core.SubSystem_core.Commands.DeleteSubSystem;

public record DeleteSubSystemCommand(Guid GUID) : IRequest;

public class DeleteSubSystemCommandHandler : IRequestHandler<DeleteSubSystemCommand>
{
    private readonly SubSystemService SubSystemService;

    public DeleteSubSystemCommandHandler(SubSystemService SubSystemService)
    {
        this.SubSystemService = SubSystemService;
    }

    public async Task Handle(DeleteSubSystemCommand request, CancellationToken cancellationToken)
    {
        var entity = await SubSystemService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        SubSystemService.Table.Remove(entity);

        await SubSystemService.SaveChangesAsync(cancellationToken);
    }
}