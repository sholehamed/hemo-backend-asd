
namespace Application.Core.ShoppingCartProduct_core.Commands.UpdateShoppingCartProduct;

public record UpdateShoppingCartProductCommand : IRequest, IMapFrom<ShoppingCartProduct>
{
 
   [PropConfig("پکیج")]
   public Guid PackageGUID { get; init; }
 
   [PropConfig("سبد خرید")]
   public Guid ShoppingCartGUID { get; init; }
 
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateShoppingCartProductCommand, ShoppingCartProduct>();
    }
}

public class UpdateShoppingCartProductCommandHandler : IRequestHandler<UpdateShoppingCartProductCommand>
{
    private ShoppingCartProductService ShoppingCartProductService;

    public UpdateShoppingCartProductCommandHandler(ShoppingCartProductService ShoppingCartProductService)
    {
        this.ShoppingCartProductService = ShoppingCartProductService;
    }

    public async Task Handle(UpdateShoppingCartProductCommand request, CancellationToken cancellationToken)
    {
        ShoppingCartProduct? OldEntity = await ShoppingCartProductService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ShoppingCartProductService.mapper.Map(request, OldEntity);
        
        await ShoppingCartProductService.SaveChangesAsync(cancellationToken);
    }
}

