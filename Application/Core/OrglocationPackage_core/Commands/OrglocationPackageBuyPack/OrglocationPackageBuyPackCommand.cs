
using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageBuyPack;

public record OrglocationPackageBuyPackCommand : IRequest, IMapFrom<OrglocationPackage>
{
 
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
 
   [PropConfig("پکیج")]
   public Guid PackageGUID { get; init; }
   

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackageBuyPackCommand, OrglocationPackage>();
    }
}

public class OrglocationPackageBuyPackCommandHandler : IRequestHandler<OrglocationPackageBuyPackCommand>
{
    private OrglocationPackageService OrglocationPackageService;
    private UserRoleService UserRoleService;

    public OrglocationPackageBuyPackCommandHandler(OrglocationPackageService orglocationPackageService, UserRoleService userRoleService)
    {
        OrglocationPackageService = orglocationPackageService;
        UserRoleService = userRoleService;
    }

    public async Task Handle(OrglocationPackageBuyPackCommand request, CancellationToken cancellationToken)
    {

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        OrgLocation? orgLocation = OrglocationPackageService.db.OrgLocation.FirstOrDefault(a =>
            a.GUID == request.OrgLocationGUID &&
            a.Path.StartsWith(userRoleInfo.OrgLocationPath)
        );
        if (orgLocation == null)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است");
        }

        OrglocationPackage? orglocationPackage = OrglocationPackageService.db.OrglocationPackage.Include(a=>a.Package).FirstOrDefault(a=>
            a.OrgLocationGUID==request.OrgLocationGUID&&
            a.PackageGUID==request.PackageGUID
            );
        if (orglocationPackage == null)
        {
            OrglocationPackageService.db.OrglocationPackage.Add(new OrglocationPackage()
            {
                OrgLocationGUID = request.OrgLocationGUID,
                PackageGUID = request.PackageGUID,
                ExpiryDate = DateTime.Now.AddDays(3)
            });
        }
        else
        {
            orglocationPackage.ExpiryDate =orglocationPackage.ExpiryDate.AddDays(3);
        }

        Package package=OrglocationPackageService.db.Package.First(a=>a.GUID==request.PackageGUID);
        UserRole? userrole = OrglocationPackageService.db.UserRole.FirstOrDefault(a =>
            a.UserGUID == userRoleInfo.UserGuid &&
            a.OrgLocationGUID==request.OrgLocationGUID&&
            a.RoleGUID ==package.RoleGUID);

        if (userrole == null)
        {
            OrglocationPackageService.db.UserRole.Add(new UserRole()
            {
                OrgLocationGUID = request.OrgLocationGUID,
                ExpireDate = DateTime.Now.AddDays(3),
                RoleGUID = package.RoleGUID,
                UserGUID = userRoleInfo.UserGuid
                
            });
        }
        else
        {
            userrole.ExpireDate=userrole.ExpireDate.AddDays(3);
        }
        
        
        
        await OrglocationPackageService.SaveChangesAsync(cancellationToken);
    }
}

