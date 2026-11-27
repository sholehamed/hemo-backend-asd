
namespace Application.Core.OrglocationPackage_core.Commands.UpdateOrglocationPackage;

public record UpdateOrglocationPackageCommand : IRequest, IMapFrom<OrglocationPackage>
{
 
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
 
   [PropConfig("پکیج")]
   public Guid PackageGUID { get; init; }
 
   [PropConfig("اعتبار")]
   public DateTime ExpiryDate { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateOrglocationPackageCommand, OrglocationPackage>();
    }
}

public class UpdateOrglocationPackageCommandHandler : IRequestHandler<UpdateOrglocationPackageCommand>
{
    private OrglocationPackageService OrglocationPackageService;

    public UpdateOrglocationPackageCommandHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task Handle(UpdateOrglocationPackageCommand request, CancellationToken cancellationToken)
    {
        OrglocationPackage? OldEntity = await OrglocationPackageService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OrglocationPackageService.mapper.Map(request, OldEntity);
        
        await OrglocationPackageService.SaveChangesAsync(cancellationToken);
    }
}

