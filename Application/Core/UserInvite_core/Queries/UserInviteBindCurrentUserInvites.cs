using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.UserInvite_core.Queries;

public record UserInviteBindCurrentUserInvitesQuery : PageFilter<UserInvite, UserInviteBindCurrentUserInvitesDTO>
{
}

public record UserInviteBindCurrentUserInvitesDTO : IMapFrom<UserInvite>
{
    [PropConfig("شرکت")] public string OrgLocationName { get; init; }
    [PropConfig("وضعیت تایید")] public bool? UserAccepted { get; init; }
    [PropConfig("تاریخ ایجاد")] public DateTime Created { get; init; }
    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserInvite, UserInviteBindCurrentUserInvitesDTO>()
 ;
    }
}

public class
    UserInviteBindCurrentUserInvitesQueryHandler : IRequestHandler<UserInviteBindCurrentUserInvitesQuery,
    PaginatedList<UserInviteBindCurrentUserInvitesDTO>>
{
    private AuthService AuthService;
    private UserInviteService UserInviteService;

    public UserInviteBindCurrentUserInvitesQueryHandler(AuthService authService, UserInviteService userInviteService)
    {
        AuthService = authService;
        UserInviteService = userInviteService;
    }

    public async Task<PaginatedList<UserInviteBindCurrentUserInvitesDTO>> Handle(
        UserInviteBindCurrentUserInvitesQuery request,
        CancellationToken cancellationToken)
    {
        Guid UserGuid = AuthService.GetUserGUID();
        IQueryable<UserInvite> queryable = UserInviteService.Table.Where(a =>
            a.UserGUID == UserGuid &&
            a.OrgLocationAccepted == true);
        return await request.ToPaging(queryable, UserInviteService.mapper);
    }
}