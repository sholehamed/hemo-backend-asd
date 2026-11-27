
namespace Application.Core.OrgType_core.Commands.UpdateOrgType;

public record UpdateOrgTypeCommand : IRequest, IMapFrom<OrgType>
{
 
   [PropConfig("نوع شرکت")]
   public Guid OrgLocationTypeGUID { get; init; }
 
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateOrgTypeCommand, OrgType>();
    }
}

public class UpdateOrgTypeCommandHandler : IRequestHandler<UpdateOrgTypeCommand>
{
    private OrgTypeService OrgTypeService;

    public UpdateOrgTypeCommandHandler(OrgTypeService OrgTypeService)
    {
        this.OrgTypeService = OrgTypeService;
    }

    public async Task Handle(UpdateOrgTypeCommand request, CancellationToken cancellationToken)
    {
        OrgType? OldEntity = await OrgTypeService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OrgTypeService.mapper.Map(request, OldEntity);
        
        await OrgTypeService.SaveChangesAsync(cancellationToken);
    }
}

