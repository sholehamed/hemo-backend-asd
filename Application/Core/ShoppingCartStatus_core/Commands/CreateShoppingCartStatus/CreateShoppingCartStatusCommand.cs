
namespace Application.Core.ShoppingCartStatus_core.Commands.CreateShoppingCartStatus;

public record CreateShoppingCartStatusCommand : IRequest<Guid>, IMapFrom<ShoppingCartStatus>
{
     
   [PropConfig("عنوان")]
   public string Name { get; init; }
    
   [PropConfig("کد")]
   public int Gkey { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateShoppingCartStatusCommand, ShoppingCartStatus>();
    }
}

public class CreateShoppingCartStatusCommandHandler : IRequestHandler<CreateShoppingCartStatusCommand, Guid>
{
    private ShoppingCartStatusService ShoppingCartStatusService;
    public CreateShoppingCartStatusCommandHandler(ShoppingCartStatusService  ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService  = ShoppingCartStatusService;
    }

    public async Task<Guid> Handle(CreateShoppingCartStatusCommand request, CancellationToken cancellationToken)
    {
        ShoppingCartStatus entity = ShoppingCartStatusService.mapper.Map<ShoppingCartStatus>(request);

        ShoppingCartStatusService.Table.Add(entity);

        await ShoppingCartStatusService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


