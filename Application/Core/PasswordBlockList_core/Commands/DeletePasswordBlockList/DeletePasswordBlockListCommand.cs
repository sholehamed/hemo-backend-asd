
namespace Application.Core.PasswordBlockList_core.Commands.DeletePasswordBlockList;

public record DeletePasswordBlockListCommand(Guid GUID) : IRequest;

public class DeletePasswordBlockListCommandHandler : IRequestHandler<DeletePasswordBlockListCommand>
{
    private PasswordBlockListService PasswordBlockListService;

    public DeletePasswordBlockListCommandHandler(PasswordBlockListService PasswordBlockListService)
    {
        this.PasswordBlockListService = PasswordBlockListService;
    }

    public async Task Handle(DeletePasswordBlockListCommand request, CancellationToken cancellationToken)
    {
        var entity = await PasswordBlockListService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        PasswordBlockListService.Table.Remove(entity);

        await PasswordBlockListService.SaveChangesAsync(cancellationToken);
    }

}

