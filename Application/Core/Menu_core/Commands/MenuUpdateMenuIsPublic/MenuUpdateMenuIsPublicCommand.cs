namespace Application.Core.Menu_core.Commands.MenuUpdateMenuIsPublic;

public record MenuUpdateMenuIsPublicCommand : IRequest, IMapFrom<Menu>
{
    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuUpdateMenuIsPublicCommand, Menu>();
    }
}

public class MenuUpdateMenuIsPublicCommandHandler : IRequestHandler<MenuUpdateMenuIsPublicCommand>
{
    private MenuService MenuService;

    public MenuUpdateMenuIsPublicCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task Handle(MenuUpdateMenuIsPublicCommand request, CancellationToken cancellationToken)
    {
        Menu? OldEntity = await MenuService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        OldEntity.IsPublic = !OldEntity.IsPublic;

        await MenuService.SaveChangesAsync(cancellationToken);
    }
}