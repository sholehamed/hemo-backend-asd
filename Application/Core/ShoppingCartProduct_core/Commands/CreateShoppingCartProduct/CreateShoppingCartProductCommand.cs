using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.ShoppingCartProduct_core.Commands.CreateShoppingCartProduct;

public record CreateShoppingCartProductCommand : IRequest<Guid>, IMapFrom<ShoppingCartProduct>
{
    [PropConfig("پکیج")] public Guid PackageGUID { get; init; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; init; }
    [PropConfig("تمدید")] public bool Renew { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateShoppingCartProductCommand, ShoppingCartProduct>();
    }
}

public class CreateShoppingCartProductCommandHandler : IRequestHandler<CreateShoppingCartProductCommand, Guid>
{
    private ShoppingCartProductService ShoppingCartProductService;
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;

    public CreateShoppingCartProductCommandHandler(ShoppingCartProductService shoppingCartProductService,
        UserRoleService userRoleService, OrgLocationService orgLocationService)
    {
        ShoppingCartProductService = shoppingCartProductService;
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
    }

    public async Task<Guid> Handle(CreateShoppingCartProductCommand request, CancellationToken cancellationToken)
    {
        await OrgLocationService.OrgLocationIsValid(request.OrgLocationGUID, true);

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        ShoppingCartProduct entity = ShoppingCartProductService.mapper.Map<ShoppingCartProduct>(request);

        ShoppingCart? ShoppingCart =
            ShoppingCartProductService.db.ShoppingCart.FirstOrDefault(a =>
                a.UserGUID == userRoleInfo.UserGuid &&
                a.ShoppingCartStatus.Gkey == 1
            );

        if (ShoppingCart == null)
        {
            ShoppingCart = new ShoppingCart()
            {
                UserGUID = userRoleInfo.UserGuid,
                ShoppingCartStatusGUID = ShoppingCartProductService.db.ShoppingCartStatus.First(a => a.Gkey == 1).GUID,
            };
            ShoppingCartProductService.db.ShoppingCart.Add(ShoppingCart);
        }
        else
        {
            bool Exist = ShoppingCartProductService.Table.Any(a =>
                a.OrgLocationGUID == request.OrgLocationGUID &&
                a.PackageGUID == request.PackageGUID &&
                a.ShoppingCartGUID == ShoppingCart.GUID);
            if (Exist)
            {
                throw new ServiceExeption("محصول انتخاب شده در سبد خرید موجود می باشد");
            }
        }

        if (request.Renew)
        {
            bool RenewIsValid = ShoppingCartProductService.db.OrglocationPackage.Any(a =>
                a.PackageGUID == request.PackageGUID && a.OrgLocationGUID == userRoleInfo.OrgLocationGUID);

            if (!RenewIsValid)
            {
                throw new ServiceExeption("این پیکج قابل تمدید نمی باشد.");
            }
            entity.Renew=ShoppingCartProductService.db.OrglocationPackage.Any(a =>
                a.PackageGUID == request.PackageGUID && a.OrgLocationGUID == userRoleInfo.OrgLocationGUID&&a.DemoRequestGUID==null);
        }

        entity.ShoppingCartGUID = ShoppingCart.GUID;

        ShoppingCartProductService.Table.Add(entity);

        await ShoppingCartProductService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}