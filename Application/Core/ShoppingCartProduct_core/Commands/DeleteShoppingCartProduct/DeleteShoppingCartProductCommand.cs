using Application.Common.Models;

namespace Application.Core.ShoppingCartProduct_core.Commands.DeleteShoppingCartProduct;

public record DeleteShoppingCartProductCommand(Guid GUID) : IRequest;

public class DeleteShoppingCartProductCommandHandler : IRequestHandler<DeleteShoppingCartProductCommand>
{
    private ShoppingCartProductService ShoppingCartProductService;
    UserRoleService UserRoleService;

    public DeleteShoppingCartProductCommandHandler(ShoppingCartProductService shoppingCartProductService,
        UserRoleService userRoleService)
    {
        ShoppingCartProductService = shoppingCartProductService;
        UserRoleService = userRoleService;
    }

    public async Task Handle(DeleteShoppingCartProductCommand request, CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        var entity = ShoppingCartProductService.Table.FirstOrDefault(a =>
            a.ShoppingCart.UserGUID == userRoleInfo.UserGuid &&
            a.ShoppingCart.ShoppingCartStatus.Gkey == 1 &&
            a.PackageGUID == request.GUID
        );

        if (entity == null)
        {
            return;
        }

        ShoppingCartProductService.Table.Remove(entity);

        await ShoppingCartProductService.SaveChangesAsync(cancellationToken);
    }
}