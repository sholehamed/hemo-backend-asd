namespace Application.Core.Menu_core.Commands.CreateMenu;

public record CreateMenuCommand : IRequest<Guid>, IMapFrom<Menu>
{
    [PropConfig("منو پدر")] public Guid? PMenuGUID { get; init; }

    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; init; }

    [PropConfig("نمایش در منو")] public bool ShowInMenu { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateMenuCommand, Menu>();
    }
}

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Guid>
{
    private readonly MenuService MenuService;

    public CreateMenuCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task<Guid> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var entity = MenuService.mapper.Map<Menu>(request);
      

     MenuService.Table.Add(entity);

        await MenuService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}