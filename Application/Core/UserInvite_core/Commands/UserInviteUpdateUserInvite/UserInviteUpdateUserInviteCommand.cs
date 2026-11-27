
using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.UserInvite_core.Commands.UserInviteUpdateUserInvite;

public record UserInviteUpdateUserInviteCommand : IRequest, IMapFrom<UserInvite>
{
   [PropConfig("قبول کردن کاربر")]
   public bool? UserAccepted { get; init; }
   
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserInviteUpdateUserInviteCommand, UserInvite>();
    }
}

public class UserInviteUpdateUserInviteCommandHandler : IRequestHandler<UserInviteUpdateUserInviteCommand>
{
    private AuthService AuthService;
    private UserInviteService UserInviteService;

    public UserInviteUpdateUserInviteCommandHandler(AuthService authService, UserInviteService userInviteService)
    {
        AuthService = authService;
        UserInviteService = userInviteService;
    }

    public async Task Handle(UserInviteUpdateUserInviteCommand request, CancellationToken cancellationToken)
    {
        UserInvite? OldEntity = await UserInviteService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        Guid UserGuid = AuthService.GetUserGUID();
        if (OldEntity.UserGUID != UserGuid)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است.");
        }
        
        UserInviteService.mapper.Map(request, OldEntity);
        
        await UserInviteService.SaveChangesAsync(cancellationToken);
    }
}

