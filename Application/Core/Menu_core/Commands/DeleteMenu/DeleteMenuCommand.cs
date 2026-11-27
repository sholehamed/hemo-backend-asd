namespace Application.Core.Menu_core.Commands.DeleteMenu;

public record DeleteMenuCommand(Guid GUID) : IRequest;

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand>
{
    private readonly MenuService MenuService;

    public DeleteMenuCommandHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        var entity = await MenuService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        MenuService.Table.Remove(entity);
        
        List<Menu>Cilds=MenuService.Table.Where(a=>a.Path.StartsWith(entity.Path)).ToList();
        foreach (Menu item in Cilds)
        {
            MenuService.Table.Remove(item);
        }

        await MenuService.SaveChangesAsync(cancellationToken);
    }
}