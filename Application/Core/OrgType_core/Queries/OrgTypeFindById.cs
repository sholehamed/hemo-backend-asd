
namespace Application.Core.OrgType_core.Queries;

public record OrgTypeFindByIdQuery(Guid GUID) : IRequest<OrgTypeFindByIdDTO>;

public record OrgTypeFindByIdDTO : IMapFrom<OrgType>
{
    
    [PropConfig("نوع شرکت")]
    public Guid OrgLocationTypeGUID { get; init; }
    
    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgType, OrgTypeFindByIdDTO>();
    }
}

public class
    OrgTypeFindByIdQueryHandler : IRequestHandler<OrgTypeFindByIdQuery, OrgTypeFindByIdDTO>
{
    private OrgTypeService OrgTypeService;

    public OrgTypeFindByIdQueryHandler(OrgTypeService OrgTypeService)
    {
        this.OrgTypeService = OrgTypeService;
    }

    public async Task<OrgTypeFindByIdDTO> Handle(OrgTypeFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        OrgTypeFindByIdDTO? Data = OrgTypeService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<OrgTypeFindByIdDTO>(OrgTypeService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

