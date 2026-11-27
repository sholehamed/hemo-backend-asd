
namespace Application.Core.ShoppingCartStatus_core.Commands.DeleteShoppingCartStatus;

public record DeleteShoppingCartStatusCommand(Guid GUID) : IRequest;

public class DeleteShoppingCartStatusCommandHandler : IRequestHandler<DeleteShoppingCartStatusCommand>
{
    private ShoppingCartStatusService ShoppingCartStatusService;

    public DeleteShoppingCartStatusCommandHandler(ShoppingCartStatusService ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService = ShoppingCartStatusService;
    }

    public async Task Handle(DeleteShoppingCartStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await ShoppingCartStatusService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        ShoppingCartStatusService.Table.Remove(entity);

        await ShoppingCartStatusService.SaveChangesAsync(cancellationToken);
    }

}

