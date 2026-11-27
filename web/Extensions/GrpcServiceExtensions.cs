

using System;
using System.Linq;
using System.Reflection;
using web.GRPCServerServices;

namespace web.Extensions;
public static class GrpcServiceExtensions
{

    public static void MapAllGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<MenuApiGRPC>();
        app.MapGrpcService<UserRoleGRPC>();
        app.MapGrpcService<OrgLocationGRPC>();
        app.MapGrpcService<TokenGRPC>();
    }
}
