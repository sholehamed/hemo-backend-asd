
namespace Application.Core.MenuApi_core.Commands.DeleteMenuApi;

public record DeleteMenuApiCommand(Guid GUID) : IRequest;

public class DeleteMenuApiCommandHandler : IRequestHandler<DeleteMenuApiCommand>
{
    private MenuApiService MenuApiService;

    public DeleteMenuApiCommandHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task Handle(DeleteMenuApiCommand request, CancellationToken cancellationToken)
    {
        var entity = await MenuApiService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        MenuApiService.Table.Remove(entity);

        List<RoleAccess> RoleAccessList = MenuApiService.db.RoleAccess.Where(a=>a.MenuGUID==entity.MenuGUID&&a.ApiGUID==entity.ApiGUID).ToList();

        MenuApiService.db.RoleAccess.RemoveRange(RoleAccessList);
        
        await MenuApiService.SaveChangesAsync(cancellationToken);
    }

}

