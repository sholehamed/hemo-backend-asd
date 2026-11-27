
namespace Application.Core.ShoppingCartStatus_core.Commands.UpdateShoppingCartStatus;

public record UpdateShoppingCartStatusCommand : IRequest, IMapFrom<ShoppingCartStatus>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
 
   [PropConfig("کد")]
   public int Gkey { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateShoppingCartStatusCommand, ShoppingCartStatus>();
    }
}

public class UpdateShoppingCartStatusCommandHandler : IRequestHandler<UpdateShoppingCartStatusCommand>
{
    private ShoppingCartStatusService ShoppingCartStatusService;

    public UpdateShoppingCartStatusCommandHandler(ShoppingCartStatusService ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService = ShoppingCartStatusService;
    }

    public async Task Handle(UpdateShoppingCartStatusCommand request, CancellationToken cancellationToken)
    {
        ShoppingCartStatus? OldEntity = await ShoppingCartStatusService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ShoppingCartStatusService.mapper.Map(request, OldEntity);
        
        await ShoppingCartStatusService.SaveChangesAsync(cancellationToken);
    }
}

