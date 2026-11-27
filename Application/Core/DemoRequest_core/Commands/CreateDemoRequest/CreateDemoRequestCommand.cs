using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.DemoRequest_core.Commands.CreateDemoRequest;

public record CreateDemoRequestCommand : IRequest<Guid>, IMapFrom<DemoRequest>
{
    [PropConfig("پکیج")] public Guid PackageGUID { get; init; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateDemoRequestCommand, DemoRequest>();
    }
}

public class CreateDemoRequestCommandHandler : IRequestHandler<CreateDemoRequestCommand, Guid>
{
    private DemoRequestService DemoRequestService;
    private AuthService AuthService;

    public CreateDemoRequestCommandHandler(DemoRequestService demoRequestService, AuthService authService)
    {
        DemoRequestService = demoRequestService;
        AuthService = authService;
    }

    public async Task<Guid> Handle(CreateDemoRequestCommand request, CancellationToken cancellationToken)
    {
        Guid UserGUID = AuthService.GetUserGUID();
        bool Exist = DemoRequestService.Table.Any(a =>
            a.Accepted != true &&
            a.UserGUID == UserGUID &&
            a.PackageGUID == request.PackageGUID);

        if (Exist)
        {
            throw new ServiceExeption("شما قبلا برای این پکیج درخواست دمو ارسال کرده اید.");
        }

        bool OrglocationPackageExist = DemoRequestService.db.OrglocationPackage.Any(a =>
            a.OrgLocationGUID == request.OrgLocationGUID && a.PackageGUID==request.PackageGUID && a.DemoRequestGUID == null);

        if (OrglocationPackageExist)
        {
            throw new ServiceExeption("درخواست دمو برای این پکیج فعال نمی باشد.");
        }
        
        DemoRequest entity = DemoRequestService.mapper.Map<DemoRequest>(request);
        entity.Accepted = null;
        entity.UserGUID = UserGUID;
        DemoRequestService.Table.Add(entity);

        await DemoRequestService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}