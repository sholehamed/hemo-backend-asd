using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Repository.Services;
using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using Role = Web.Endpoints.Role;

namespace web.GRPCServerServices;

public class UserRoleGRPC : UserRoleGRPCService.UserRoleGRPCServiceBase
{
    private IApplicationDbContext db;
    UserRoleService UserRoleService;
    private UserService UserService;

    public UserRoleGRPC(IApplicationDbContext db, UserRoleService userRoleService, UserService userService)
    {
        this.db = db;
        UserRoleService = userRoleService;
        UserService = userService;
    }

    public async override Task<GetUserListByRoleIDsReply> GetUserListByRoleIDs(GetUserListByRoleIDsRequest request,
        ServerCallContext context)
    {
        Guid OrglocationGUID = Guid.Parse(request.OrglocationGUID);
        List<Guid> RoleGUIDs = JsonConvert.DeserializeObject<List<Guid>>(request.RoleGUIDs);
        List<string> Data = db.UserRole
            .Where(a => a.OrgLocationGUID == OrglocationGUID && RoleGUIDs.Any(b => b == a.RoleGUID))
            .Select(a => a.OrgLocation.Name + "@" + a.Role.Name + "@" + a.User.FirstName + "@" + a.User.LastName + "@" + a.User.Mobile + "@" +
                         a.User.GUID)
            .ToList();

        return new GetUserListByRoleIDsReply()
        {
            Data = JsonConvert.SerializeObject(Data)
        };
    }

    public async override Task<GetRoleInfoReply> GetRoleInfo(GetRoleInfoRequest request, ServerCallContext context)
    {
        Domain.Entities.Role role = db.Role.First(a => a.GUID == Guid.Parse(request.GUID));

        GetRoleInfoReply GetRoleInfoReply = new GetRoleInfoReply()
        {
            Gkey = role.Gkey,
            Level = role.Level,
            OrgLocationGUID = (role.OrgLocationGUID != null) ? role.OrgLocationGUID.ToString() : Guid.Empty.ToString(),
            Name = role.Name ?? "",
            Path = role.Path ?? "",
            PRoleGUID = (role.PRoleGUID != null) ? role.PRoleGUID.ToString() : Guid.Empty.ToString(),
            OrgLocationTypeGUID = (role.OrgLocationTypeGUID != null)
                ? role.OrgLocationTypeGUID.ToString()
                : Guid.Empty.ToString(),
        };
        return GetRoleInfoReply;
    }

    public async override Task<RemoveUserRoleReply> RemoveUserRole(RemoveUserRoleRequest request,
        ServerCallContext context)
    {
        UserRole? UserRole = db.UserRole.FirstOrDefault(a =>
            a.Role.Gkey == request.RoleGkey &&
            a.OrgLocationGUID == Guid.Parse(request.OrgLocationGUID) &&
            a.UserGUID == Guid.Parse(request.UserGUID));

        bool Success = false;
        if (UserRole != null)
        {
            Success = true;
            db.UserRole.Remove(UserRole);
            await db.SaveChangesAsync(CancellationToken.None);
        }

        RemoveUserRoleReply RemoveUserRoleReply = new RemoveUserRoleReply()
        {
            IsSuccess = Success,
        };
        return RemoveUserRoleReply;
    }

    public override async Task<CreateUserRoleReply> CreateUserRole(CreateUserRoleRequest request,
        ServerCallContext context)
    {
        UserRole? UserRole = db.UserRole.FirstOrDefault(a =>
            a.Role.Gkey == request.RoleGkey &&
            a.OrgLocationGUID == Guid.Parse(request.OrgLocationGUID) &&
            a.UserGUID == Guid.Parse(request.UserGUID));

        DateTime ExpireDate = new DateTime(request.ExpireDate);
        if (UserRole == null)
        {
            UserRole = new UserRole()
            {
                RoleGUID = db.Role.First(a => a.Gkey == request.RoleGkey).GUID,
                OrgLocationGUID = Guid.Parse(request.OrgLocationGUID),
                UserGUID = Guid.Parse(request.UserGUID),
                ExpireDate = ExpireDate,
            };
            db.UserRole.Add(UserRole);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        else
        {
            UserRole.ExpireDate = ExpireDate;
            db.UserRole.Update(UserRole);
            await db.SaveChangesAsync(CancellationToken.None);
        }

        CreateUserRoleReply CreateUserRoleReply = new CreateUserRoleReply()
        {
            UserRoleGkey = UserRole.Gkey
        };

        return CreateUserRoleReply;
    }

    public async override Task<GetUserInfoReply> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        User user = db.User.First(a => a.GUID == Guid.Parse(request.UserGUID));
        GetUserInfoReply GetUserInfoReply = new GetUserInfoReply()
        {
            Email = user.Email ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Mobile = user.Mobile ?? "",
            Username = user.Username ?? "",
            UserGUID = request.UserGUID
        };
        return GetUserInfoReply;
    }

    public async override Task<CreateUserReply> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        CreateUserReply Reply = new CreateUserReply()
        {
            UserGuid = "",
            OrgLocationGuid = "",
            Success = false,
            ErrorMessage = "",
        };

        try
        {
            var result = await UserService.CreateUser(request.Username, request.Password, request.FirstName,
                request.LastName,
                request.Email, request.Mobile, Guid.Parse("0A5E5228-9C6C-4B81-BCA4-9BE6363C7AAE"),
                $"پروفایل شخصی {request.FirstName} {request.LastName}",
                "", "", "", "", "", DateTime.Now.AddYears(100),
                true,request.Nationalcode,
                CancellationToken.None);
            Reply.UserGuid = result.UserGUID.ToString();
            Reply.OrgLocationGuid = result.OrglocationGUID.ToString();
            return Reply;
        }
        catch (ServiceExeption e)
        {
            Reply.ErrorMessage = e.Message;
            return Reply;
        }
        catch (Exception e)
        {
            Reply.ErrorMessage = "خطای سرور رخ داد";
            return Reply;
        }

        return Reply;
    }

    public override async Task<GetUserRoleInfoReply> GetUserRoleInfo(GetUserRoleInfoRequest request,
        ServerCallContext context)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfoByGkey(request.UserRoleGkey);

        GetUserRoleInfoReply GetUserRoleInfoReply = new GetUserRoleInfoReply()
        {
            ExpireDate = userRoleInfo.ExpireDate.Ticks,
            IsSuperAdmin = userRoleInfo.IsSuperAdmin,
            OrgLocationGkey = userRoleInfo.OrgLocationGkey,
            OrgLocationGUID = userRoleInfo.OrgLocationGUID.ToString(),
            OrgLocationPath = userRoleInfo.OrgLocationPath,
            RoleGkey = userRoleInfo.RoleGkey,
            RoleGuid = userRoleInfo.RoleGuid.ToString(),
            RoleName = userRoleInfo.RoleName,
            RolePath = userRoleInfo.RolePath,
            UserFirstName = userRoleInfo.UserFirstName,
            UserLastName = userRoleInfo.UserLastName,
            UserGuid = userRoleInfo.UserGuid.ToString(),
        };
        return GetUserRoleInfoReply;
    }

    public override async Task<GetOrInsertUserWithUserRoleResponse> GetOrInsertUserWithUserRole(
        GetOrInsertUserWithUserRoleRequest request, ServerCallContext context)
    {
        var userGuid = await UserService.GetOrInsertUserWithUserRole(request.Username, request.Password,
            request.FirstName, request.LastName, request.Email,
            request.Mobile, Guid.Parse(request.OrgLocationGUID), request.RoleGkey, DateTime.Parse(request.ExpireDate),
            CancellationToken.None);

        return new GetOrInsertUserWithUserRoleResponse()
        {
            UserGuid = userGuid.ToString()
        };
    }
}