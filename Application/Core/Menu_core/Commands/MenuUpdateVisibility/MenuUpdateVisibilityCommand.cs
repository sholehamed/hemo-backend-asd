
namespace Application.Core.Menu_core.Commands.MenuUpdateVisibility;

public record MenuUpdateVisibilityCommand : IRequest, IMapFrom<Menu>
{
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuUpdateVisibilityCommand, Menu>();
    }
}

public class MenuUpdateVisibilityCommandHandler : IRequestHandler<MenuUpdateVisibilityCommand>
{
    private MenuService MenuService;

    public MenuUpdateVisibilityCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task Handle(MenuUpdateVisibilityCommand request, CancellationToken cancellationToken)
    {
        Menu? OldEntity = await MenuService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OldEntity.ShowInMenu=!OldEntity.ShowInMenu;
        
        await MenuService.SaveChangesAsync(cancellationToken);
    }
}

