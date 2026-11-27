using Application.Common.Models;

namespace Application.Core.User_core.Queries;

public record UserGetERPDashboardQuery(Guid GUID) : IRequest<UserGetERPDashboardDTO>;

public record UserGetERPDashboardDTO : IMapFrom<User>
{
    [PropConfig("تعداد شرکت")] public long OrglocationCount { get; init; }

    [PropConfig("دعوت نامه های بررسی نشده")]
    public long ActiveUserInviteCount { get; init; }

    [PropConfig("تعداد دعوت نامه ها")] public long UserInviteCount { get; init; }

    [PropConfig("تعداد کل فاکتور ها")] public long InvoiceCount { get; init; }

    [PropConfig("تعداد محصول در فاکتور فعال")]
    public long ActiveInvoiceCount { get; init; }

    [PropConfig("تعداد اعلانات")] public long NotificationCount { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserGetERPDashboardDTO>();
    }
}

public class
    UserGetERPDashboardQueryHandler : IRequestHandler<UserGetERPDashboardQuery, UserGetERPDashboardDTO>
{
    private UserService UserService;
    private ISender sender;

    private UserRoleService UserRoleService;

    public UserGetERPDashboardQueryHandler(UserService userService, ISender sender, UserRoleService userRoleService)
    {
        UserService = userService;
        this.sender = sender;
        UserRoleService = userRoleService;
    }

    public async Task<UserGetERPDashboardDTO> Handle(UserGetERPDashboardQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();


        UserGetERPDashboardDTO Data = new UserGetERPDashboardDTO()
        {
            InvoiceCount = UserService.db.ShoppingCart.Count(a => a.UserGUID == userRoleInfo.UserGuid),
            NotificationCount = 0,
            OrglocationCount = UserService.db.OrgLocation.Count(a =>
                a.Path != userRoleInfo.OrgLocationPath && a.Path.StartsWith(userRoleInfo.OrgLocationPath)),
            UserInviteCount = UserService.db.UserInvite.Count(a => a.UserGUID == userRoleInfo.UserGuid),
            ActiveUserInviteCount = UserService.db.UserInvite.Count(a =>
                (a.UserAccepted == null || a.OrgLocationAccepted == null) && a.UserGUID == userRoleInfo.UserGuid),
            
        };
        return Data;
    }
}