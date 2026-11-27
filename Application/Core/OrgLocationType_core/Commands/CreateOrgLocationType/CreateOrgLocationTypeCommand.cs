
namespace Application.Core.OrgLocationType_core.Commands.CreateOrgLocationType;

public record CreateOrgLocationTypeCommand : IRequest<Guid>, IMapFrom<OrgLocationType>
{
     
   [PropConfig("عنوان")]
   public string Name { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOrgLocationTypeCommand, OrgLocationType>();
    }
}

public class CreateOrgLocationTypeCommandHandler : IRequestHandler<CreateOrgLocationTypeCommand, Guid>
{
    private OrgLocationTypeService OrgLocationTypeService;
    public CreateOrgLocationTypeCommandHandler(OrgLocationTypeService  OrgLocationTypeService)
    {
        this.OrgLocationTypeService  = OrgLocationTypeService;
    }

    public async Task<Guid> Handle(CreateOrgLocationTypeCommand request, CancellationToken cancellationToken)
    {
        OrgLocationType entity = OrgLocationTypeService.mapper.Map<OrgLocationType>(request);

        OrgLocationTypeService.Table.Add(entity);

        await OrgLocationTypeService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


