using Application.Common.Exceptions;
using Application.Common.Models;
using System;

namespace Application.Repository.Services;

public class RoleService : BaseRepository<Role>
{
    private UserRoleService UserRoleService;

    public RoleService(IApplicationDbContext context, IMapper mapper, UserRoleService userRoleService) : base(context,
        mapper)
    {
        UserRoleService = userRoleService;
    }

    public async Task<bool> RoleIsValid(Guid? RoleGuid)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        if (userRoleInfo.IsSuperAdmin)
        {
            return true;
        }
        bool Valid = Table.Any(a =>
            a.GUID == RoleGuid &&
             a.Path != userRoleInfo.RolePath &&
            a.Path.StartsWith(userRoleInfo.RolePath));

        if (!Valid)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است.");
        }
        else
        {
            return Valid;
        }
    }


    public async Task InserDefaults(CancellationToken cancellationToken)
    {
        if (!Table.Any())
        {
            Table.Add(new Role
            {
                Name = "ادمین",
                Gkey = 1,
                Path = "/1/"
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}