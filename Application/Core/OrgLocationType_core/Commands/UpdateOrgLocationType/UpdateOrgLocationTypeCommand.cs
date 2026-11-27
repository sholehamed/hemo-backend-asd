
namespace Application.Core.OrgLocationType_core.Commands.UpdateOrgLocationType;

public record UpdateOrgLocationTypeCommand : IRequest, IMapFrom<OrgLocationType>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateOrgLocationTypeCommand, OrgLocationType>();
    }
}

public class UpdateOrgLocationTypeCommandHandler : IRequestHandler<UpdateOrgLocationTypeCommand>
{
    private OrgLocationTypeService OrgLocationTypeService;

    public UpdateOrgLocationTypeCommandHandler(OrgLocationTypeService OrgLocationTypeService)
    {
        this.OrgLocationTypeService = OrgLocationTypeService;
    }

    public async Task Handle(UpdateOrgLocationTypeCommand request, CancellationToken cancellationToken)
    {
        OrgLocationType? OldEntity = await OrgLocationTypeService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OrgLocationTypeService.mapper.Map(request, OldEntity);
        
        await OrgLocationTypeService.SaveChangesAsync(cancellationToken);
    }
}

