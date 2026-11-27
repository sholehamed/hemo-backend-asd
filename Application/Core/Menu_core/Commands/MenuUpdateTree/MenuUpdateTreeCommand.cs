namespace Application.Core.Menu_core.Commands.MenuUpdateTree;

public record MenuUpdateTreeCommand : IRequest, IMapFrom<Menu>
{
    [PropConfig("کلید مبدا")] public Guid SourceGUID { get; init; }
    [PropConfig("کلید مقصد")] public Guid DestGUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuUpdateTreeCommand, Menu>();
    }
}

public class MenuUpdateTreeCommandHandler : IRequestHandler<MenuUpdateTreeCommand>
{
    private MenuService MenuService;

    public MenuUpdateTreeCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task Handle(MenuUpdateTreeCommand request, CancellationToken cancellationToken)
    {
        Menu? Source = await MenuService.Table
            .FindAsync(request.SourceGUID, cancellationToken);
        Menu? Dest = await MenuService.Table
            .FindAsync(request.DestGUID, cancellationToken);

        Guard.Against.NotFound(request.SourceGUID, Source);
        Guard.Against.NotFound(request.DestGUID, Dest);

      
        if (Source.PMenuGUID == Dest.PMenuGUID)
        {
        
            int _Source = Source.Gorder;
            int _Dest = Dest.Gorder;
            Source.Gorder = _Dest;

        
            if (_Source < _Dest)
            {
                List<Menu> TreeData = MenuService.Table
                    .Where(a =>a.GUID!=Source.GUID && a.PMenuGUID == Dest.PMenuGUID && _Source <= a.Gorder && a.Gorder <= _Dest).ToList();
                foreach (var data in TreeData)
                {
                    data.Gorder--;
                }
            }
            else
            {
                List<Menu> TreeData = MenuService.Table
                    .Where(a =>a.GUID!=Source.GUID && a.PMenuGUID == Dest.PMenuGUID &&   _Dest<= a.Gorder && a.Gorder <=_Source ).ToList();
                foreach (var data in TreeData)
                {
                    data.Gorder++;
                }
            }
        }
        else
        {
            Source.PMenuGUID = Dest.PMenuGUID;
        }

        await MenuService.SaveChangesAsync(cancellationToken);

        
    }
}