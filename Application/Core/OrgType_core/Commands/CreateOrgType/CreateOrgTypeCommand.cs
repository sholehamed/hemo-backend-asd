
namespace Application.Core.OrgType_core.Commands.CreateOrgType;

public record CreateOrgTypeCommand : IRequest<Guid>, IMapFrom<OrgType>
{
     
   [PropConfig("نوع شرکت")]
   public Guid OrgLocationTypeGUID { get; init; }
    
   [PropConfig("شرکت")]
   public Guid OrgLocationGUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOrgTypeCommand, OrgType>();
    }
}

public class CreateOrgTypeCommandHandler : IRequestHandler<CreateOrgTypeCommand, Guid>
{
    private OrgTypeService OrgTypeService;
    public CreateOrgTypeCommandHandler(OrgTypeService  OrgTypeService)
    {
        this.OrgTypeService  = OrgTypeService;
    }

    public async Task<Guid> Handle(CreateOrgTypeCommand request, CancellationToken cancellationToken)
    {
        OrgType entity = OrgTypeService.mapper.Map<OrgType>(request);

        OrgTypeService.Table.Add(entity);

        await OrgTypeService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


