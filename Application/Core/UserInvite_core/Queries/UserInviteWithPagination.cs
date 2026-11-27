using Application.Common.Models;

namespace Application.Core.UserInvite_core.Queries;

public record UserInviteWithPaginationQuery : PageFilter<UserInvite, UserInviteWithPaginationDTO>
{
}

public record UserInviteWithPaginationDTO : IMapFrom<UserInvite>
{
    [PropConfig("شرکت")] public string OrgLocationName { get; init; }

    [PropConfig("وضعیت تایید کاربر")] public string UserAcceptedStatus { get; init; }

    [PropConfig("کاربر")] public string UserInfo { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserInvite, UserInviteWithPaginationDTO>()
            .ForMember(dest => dest.UserAcceptedStatus, opt =>
                opt.MapFrom(src =>
                    (src.UserAccepted == null)
                        ? "در انتظار تایید"
                        : ((src.UserAccepted == true) ? "تایید شده" : "رد شده")))
            .ForMember(dest => dest.OrgLocationName, opt => opt.MapFrom(src => src.OrgLocation.Name));
    }
}

public class
    UserInviteWithPaginationQueryHandler : IRequestHandler<UserInviteWithPaginationQuery,
    PaginatedList<UserInviteWithPaginationDTO>>
{
    private UserInviteService UserInviteService;
    private UserRoleService UserRoleService;

    public UserInviteWithPaginationQueryHandler(UserInviteService userInviteService, UserRoleService userRoleService)
    {
        UserInviteService = userInviteService;
        UserRoleService = userRoleService;
    }

    public async Task<PaginatedList<UserInviteWithPaginationDTO>> Handle(UserInviteWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<UserInvite> queryable =
            UserInviteService.Table.Where(a =>userRoleInfo.IsSuperAdmin|| a.OrgLocation.Path.StartsWith(userRoleInfo.OrgLocationPath));
        return await request.ToPaging(queryable, UserInviteService.mapper);
    }
}