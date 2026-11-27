namespace Application.Core.Menu_core.Commands.UpdateMenu;

public record UpdateMenuCommand : IRequest, IMapFrom<Menu>
{
    

    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateMenuCommand, Menu>();
    }
}

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand>
{
    private readonly MenuService MenuService;

    public UpdateMenuCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var OldEntity = await MenuService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        MenuService.mapper.Map(request, OldEntity);

        

        await MenuService.SaveChangesAsync(cancellationToken);
    }
}