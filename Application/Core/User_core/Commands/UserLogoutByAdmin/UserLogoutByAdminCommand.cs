using Application.Core.User_core.Queries;

namespace Application.Core.User_core.Commands.UserLogoutByAdmin;

public record UserLogoutByAdminCommand : IRequest<UserLogoutDTO>, IMapFrom<User>
{
    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserLogoutByAdminCommand, User>();
    }
}

public class UserLogoutByAdminCommandHandler : IRequestHandler<UserLogoutByAdminCommand, UserLogoutDTO>
{
    private UserService UserService;

    public UserLogoutByAdminCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<UserLogoutDTO> Handle(UserLogoutByAdminCommand request, CancellationToken cancellationToken)
    {
        User? OldEntity = await UserService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        UserService.Logout(request.GUID,true);

        UserLogoutDTO? Data = UserService.Table.AsNoTracking().Where(a => a.GUID == request.GUID)
            .ProjectTo<UserLogoutDTO>(UserService.mapper.ConfigurationProvider).FirstOrDefault();
        return Data;
    }
}