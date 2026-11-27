using Application.Common.Utility.Security;

namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateVisibility;

public record OrglocationPackageUpdateVisibilityCommand : IRequest, IMapFrom<OrglocationPackage>
{
    [PropConfig("پکیج پنهان باشد")] public bool Hidden { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackageUpdateVisibilityCommand, OrglocationPackage>();
    }
}

public class
    OrglocationPackageUpdateVisibilityCommandHandler : IRequestHandler<OrglocationPackageUpdateVisibilityCommand>
{
    private OrglocationPackageService OrglocationPackageService;
    private AuthService AuthService;

    public OrglocationPackageUpdateVisibilityCommandHandler(OrglocationPackageService orglocationPackageService,
        AuthService authService)
    {
        OrglocationPackageService = orglocationPackageService;
        AuthService = authService;
    }

    public async Task Handle(OrglocationPackageUpdateVisibilityCommand request, CancellationToken cancellationToken)
    {
        Guid UserGUID = AuthService.GetUserGUID();

        OrglocationPackage? OldEntity =
            OrglocationPackageService.Table.FirstOrDefault(a => a.GUID == request.GUID && a.CreatedBy == UserGUID);


        Guard.Against.NotFound(request.GUID, OldEntity);

        OrglocationPackageService.mapper.Map(request, OldEntity);

        await OrglocationPackageService.SaveChangesAsync(cancellationToken);
    }
}