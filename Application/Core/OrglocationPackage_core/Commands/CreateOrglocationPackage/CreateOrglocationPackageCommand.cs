
namespace Application.Core.OrglocationPackage_core.Commands.CreateOrglocationPackage;

public record CreateOrglocationPackageCommand : IRequest<Guid>, IMapFrom<OrglocationPackage>
{
     
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
    
   [PropConfig("پکیج")]
   public Guid PackageGUID { get; init; }
    
   [PropConfig("اعتبار")]
   public DateTime ExpiryDate { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOrglocationPackageCommand, OrglocationPackage>();
    }
}

public class CreateOrglocationPackageCommandHandler : IRequestHandler<CreateOrglocationPackageCommand, Guid>
{
    private OrglocationPackageService OrglocationPackageService;
    public CreateOrglocationPackageCommandHandler(OrglocationPackageService  OrglocationPackageService)
    {
        this.OrglocationPackageService  = OrglocationPackageService;
    }

    public async Task<Guid> Handle(CreateOrglocationPackageCommand request, CancellationToken cancellationToken)
    {
        OrglocationPackage entity = OrglocationPackageService.mapper.Map<OrglocationPackage>(request);

        OrglocationPackageService.Table.Add(entity);

        await OrglocationPackageService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


