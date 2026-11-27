using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.UserInvite_core.Commands.CreateUserInvite;

public record CreateUserInviteCommand : IRequest<Guid>, IMapFrom<UserInvite>
{
    [PropConfig("ایمیل/موبایل/نام کاربری")]
    public string UserInfo { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateUserInviteCommand, UserInvite>();
    }
}

public class CreateUserInviteCommandHandler : IRequestHandler<CreateUserInviteCommand, Guid>
{
    private UserInviteService UserInviteService;
    private UserRoleService UserRoleService;

    public CreateUserInviteCommandHandler(UserInviteService userInviteService, UserRoleService userRoleService)
    {
        UserInviteService = userInviteService;
        UserRoleService = userRoleService;
    }

    public async Task<Guid> Handle(CreateUserInviteCommand request, CancellationToken cancellationToken)
    {
        string UserInfo = request.UserInfo.Trim().ToLower();
        User? user = UserInviteService.db.User.FirstOrDefault(a =>
            a.Email == UserInfo ||
            a.Username == UserInfo ||
            a.Mobile == UserInfo
        );

        if (user == null)
        {
            throw new ServiceExeption("کاربر یافت نشد.");
        }

        UserRoleInfo UserRoleInfo = await UserRoleService.GetUserRoleInfo();

        bool UserInviteExist = UserInviteService.Table.Any(a =>
            a.UserGUID == user.GUID &&
            a.UserAccepted != false &&
            a.OrgLocationGUID == UserRoleInfo.OrgLocationGUID
        );

        if (UserInviteExist)
        {
            throw new ServiceExeption("درخواست برای این کاربر ارسال شده است.");
        }

        UserInvite entity = UserInviteService.mapper.Map<UserInvite>(request);

        entity.OrgLocationAccepted = true;
        entity.OrgLocationGUID = (Guid)UserRoleInfo.OrgLocationGUID;
        entity.UserAccepted = null;
        entity.UserGUID = user.GUID;
        UserInviteService.Table.Add(entity);

        await UserInviteService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}