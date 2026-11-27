
namespace Application.Core.UserInvite_core.Commands.UpdateUserInvite;

public record UpdateUserInviteCommand : IRequest, IMapFrom<UserInvite>
{
 
   [PropConfig("کاربر")]
   public Guid UserGUID { get; init; }
 
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
 
   [PropConfig("قبول کردن کاربر")]
   public bool? UserAccepted { get; init; }
 
   [PropConfig("قبول کردن شرکت")]
   public bool? OrgLocationAccepted { get; init; }
 
   [PropConfig("کاربر")]
   public string UserInfo { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateUserInviteCommand, UserInvite>();
    }
}

public class UpdateUserInviteCommandHandler : IRequestHandler<UpdateUserInviteCommand>
{
    private UserInviteService UserInviteService;

    public UpdateUserInviteCommandHandler(UserInviteService UserInviteService)
    {
        this.UserInviteService = UserInviteService;
    }

    public async Task Handle(UpdateUserInviteCommand request, CancellationToken cancellationToken)
    {
        UserInvite? OldEntity = await UserInviteService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        UserInviteService.mapper.Map(request, OldEntity);
        
        await UserInviteService.SaveChangesAsync(cancellationToken);
    }
}

