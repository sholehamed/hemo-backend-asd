namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateExpireDate;

public record OrglocationPackageUpdateExpireDateCommand : IRequest, IMapFrom<OrglocationPackage>
{
    [PropConfig("اعتبار")] public DateTime ExpiryDate { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackageUpdateExpireDateCommand, OrglocationPackage>();
    }
}

public class
    OrglocationPackageUpdateExpireDateCommandHandler : IRequestHandler<OrglocationPackageUpdateExpireDateCommand>
{
    private OrglocationPackageService OrglocationPackageService;

    public OrglocationPackageUpdateExpireDateCommandHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task Handle(OrglocationPackageUpdateExpireDateCommand request, CancellationToken cancellationToken)
    {
        OrglocationPackage orglocationPackage = OrglocationPackageService.db.OrglocationPackage
            .Include(a => a.Package)
            .Include(a => a.OrgLocation)
            .First(a => a.GUID == request.GUID);

        UserRole? userRole =
            OrglocationPackageService.db.UserRole.FirstOrDefault(a =>
                a.UserGUID == orglocationPackage.CreatedBy &&
                a.OrgLocationGUID == orglocationPackage.OrgLocationGUID &&
                a.RoleGUID == orglocationPackage.Package.RoleGUID);
        if (userRole is not null)
        {
            userRole.ExpireDate = request.ExpiryDate;
        }

        orglocationPackage.ExpiryDate = request.ExpiryDate;

        await OrglocationPackageService.SaveChangesAsync(cancellationToken);
    }
}