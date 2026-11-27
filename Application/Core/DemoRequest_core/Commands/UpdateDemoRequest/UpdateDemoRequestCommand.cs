using Application.Common.Exceptions;

namespace Application.Core.DemoRequest_core.Commands.UpdateDemoRequest;

public record UpdateDemoRequestCommand : IRequest, IMapFrom<DemoRequest>
{
    [PropConfig("قبول شده")] public bool Accepted { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateDemoRequestCommand, DemoRequest>();
    }
}

public class UpdateDemoRequestCommandHandler : IRequestHandler<UpdateDemoRequestCommand>
{
    private DemoRequestService DemoRequestService;

    public UpdateDemoRequestCommandHandler(DemoRequestService DemoRequestService)
    {
        this.DemoRequestService = DemoRequestService;
    }

    public async Task Handle(UpdateDemoRequestCommand request, CancellationToken cancellationToken)
    {
        DemoRequest? OldEntity = DemoRequestService.Table.Include(a => a.Package)
            .FirstOrDefault(a => a.GUID == request.GUID);

        Guard.Against.NotFound(request.GUID, OldEntity);

        if (OldEntity.Accepted == true)
        {
            throw new ServiceExeption("درخواست قبلا تایید شده است.");
        }

        DemoRequestService.mapper.Map(request, OldEntity);


        if (request.Accepted == true)
        {
            UserRole? userRole =
                DemoRequestService.db.UserRole.FirstOrDefault(a =>
                    a.UserGUID == OldEntity.UserGUID &&
                    a.OrgLocationGUID == OldEntity.OrgLocationGUID &&
                    a.RoleGUID == OldEntity.Package.RoleGUID);

            if (userRole == null)
            {
                userRole = new UserRole()
                {
                    UserGUID = OldEntity.UserGUID,
                    RoleGUID = OldEntity.Package.RoleGUID,
                    OrgLocationGUID = OldEntity.OrgLocationGUID,
                    ExpireDate = DateTime.Now.AddDays(3),
                };
                DemoRequestService.db.UserRole.Add(userRole);
            }
            else
            {
                if (userRole.ExpireDate > DateTime.Now)
                {
                    userRole.ExpireDate = userRole.ExpireDate.AddDays(3);
                }
                else
                {
                    userRole.ExpireDate = DateTime.Now.AddDays(3);
                }
            }

            OrglocationPackage? orglocationPackage = DemoRequestService.db.OrglocationPackage.FirstOrDefault(a =>
                a.OrgLocationGUID == OldEntity.OrgLocationGUID &&
                a.PackageGUID == OldEntity.PackageGUID);


            if (orglocationPackage != null)
            {
                if (orglocationPackage.DemoRequestGUID == null || !orglocationPackage.DemoRequestGUID.HasValue)
                {
                    throw new ServiceExeption("پکیج قبلا توسط کاربر خریداری شده است و امکان فعال سازی نسخه دمو وجود ندارد.");
                }
                
                orglocationPackage.ExpiryDate = userRole.ExpireDate;
                orglocationPackage.Hidden = false;
                orglocationPackage.DemoRequestGUID = OldEntity.GUID;
            }
            else
            {
                orglocationPackage = new OrglocationPackage()
                {
                    OrgLocationGUID = OldEntity.OrgLocationGUID,
                    PackageGUID = OldEntity.PackageGUID,
                    ExpiryDate = userRole.ExpireDate,
                    Hidden = false,
                    DemoRequestGUID = OldEntity.GUID
                };
                DemoRequestService.db.OrglocationPackage.Add(orglocationPackage);
            }
        }
        await DemoRequestService.SaveChangesAsync(cancellationToken);
    }

}