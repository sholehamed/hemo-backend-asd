using Application.Common.Models;
using System;

namespace Application.Core.UserRole_core.Queries;

public record UserRoleWithPaginationQuery : PageFilter<UserRole, UserRoleWithPaginationDTO>
{
    public Guid UserID { get; set; }
}

public record UserRoleWithPaginationDTO : IMapFrom<UserRole>
{
    [PropConfig("نقش")] public string RoleName { get; init; }

    [PropConfig("شرکت")] public string? OrgLocationName { get; init; }

    [PropConfig("تاریخ انقضا")] public DateTime ExpireDate { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserRole, UserRoleWithPaginationDTO>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(a => a.Role.Name))
            ;
    }
}

public class
    UserRoleWithPaginationQueryHandler : IRequestHandler<UserRoleWithPaginationQuery,
    PaginatedList<UserRoleWithPaginationDTO>>
{
    private UserRoleService UserRoleService;

    public UserRoleWithPaginationQueryHandler(UserRoleService UserRoleService)
    {
        this.UserRoleService = UserRoleService;
    }

    public async Task<PaginatedList<UserRoleWithPaginationDTO>> Handle(UserRoleWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        IQueryable<UserRole> queryable = UserRoleService.Table.Where(a =>
            a.OrgLocation.Path.StartsWith(userRoleInfo.OrgLocationPath) && 
            //a.Role.Path.StartsWith(userRoleInfo.RolePath) && 
            //a.Role.Path != userRoleInfo.RolePath && 
            a.UserGUID == request.UserID
            );
        return await request.ToPaging(queryable, UserRoleService.mapper);
    }
}