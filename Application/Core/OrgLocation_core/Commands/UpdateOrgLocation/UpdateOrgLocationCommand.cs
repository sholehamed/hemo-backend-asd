
namespace Application.Core.OrgLocation_core.Commands.UpdateOrgLocation;

public record UpdateOrgLocationCommand : IRequest, IMapFrom<OrgLocation>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
   
   [PropConfig("آدرس")]
   public string? Address { get; init; }
 
   [PropConfig("تلفن")]
   public string? Tell { get; init; }
 
   [PropConfig("فکس")]
   public string? Fax { get; init; }
 
   [PropConfig("ایمیل")]
   public string? Email { get; init; }
 
   [PropConfig("آدرس وبسایت")]
   public string? Url { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateOrgLocationCommand, OrgLocation>();
    }
}

public class UpdateOrgLocationCommandHandler : IRequestHandler<UpdateOrgLocationCommand>
{
    private OrgLocationService OrgLocationService;

    public UpdateOrgLocationCommandHandler(OrgLocationService OrgLocationService)
    {
        this.OrgLocationService = OrgLocationService;
    }

    public async Task Handle(UpdateOrgLocationCommand request, CancellationToken cancellationToken)
    {
        await OrgLocationService.OrgLocationIsValid(request.GUID);
        
        OrgLocation? OldEntity = await OrgLocationService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OrgLocationService.mapper.Map(request, OldEntity);
        
        await OrgLocationService.SaveChangesAsync(cancellationToken);
    }
}

