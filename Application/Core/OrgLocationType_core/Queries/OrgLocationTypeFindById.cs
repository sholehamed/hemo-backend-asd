
namespace Application.Core.OrgLocationType_core.Queries;

public record OrgLocationTypeFindByIdQuery(Guid GUID) : IRequest<OrgLocationTypeFindByIdDTO>;

public record OrgLocationTypeFindByIdDTO : IMapFrom<OrgLocationType>
{
   
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocationType, OrgLocationTypeFindByIdDTO>();
    }
}

public class
    OrgLocationTypeFindByIdQueryHandler : IRequestHandler<OrgLocationTypeFindByIdQuery, OrgLocationTypeFindByIdDTO>
{
    private OrgLocationTypeService OrgLocationTypeService;

    public OrgLocationTypeFindByIdQueryHandler(OrgLocationTypeService OrgLocationTypeService)
    {
        this.OrgLocationTypeService = OrgLocationTypeService;
    }

    public async Task<OrgLocationTypeFindByIdDTO> Handle(OrgLocationTypeFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        OrgLocationTypeFindByIdDTO? Data = OrgLocationTypeService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<OrgLocationTypeFindByIdDTO>(OrgLocationTypeService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

