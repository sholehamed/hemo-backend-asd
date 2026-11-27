namespace Application.Core.User_core.Commands.DeleteUser;

public record DeleteUserCommand(Guid GUID) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserService UserService;

    public DeleteUserCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await UserService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        UserService.Table.Remove(entity);

        await UserService.SaveChangesAsync(cancellationToken);
    }
}