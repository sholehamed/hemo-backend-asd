

namespace Application.Core.OrgType_core.Queries;

public record OrgTypeFindAllQuery : IRequest<List<OrgTypeFindAllDTO>>
{

}

public record OrgTypeFindAllDTO : IMapFrom<OrgType>
{

    [PropConfig("نوع شرکت")]
    public Guid OrgLocationTypeGUID { get; init; }

    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgType, OrgTypeFindAllDTO>();
    }
}

public class
    OrgTypeFindAllQueryHandler : IRequestHandler<OrgTypeFindAllQuery,
    List<OrgTypeFindAllDTO>>
{
    private OrgTypeService OrgTypeService;
    public OrgTypeFindAllQueryHandler(OrgTypeService OrgTypeService)
    {
        this.OrgTypeService = OrgTypeService;
    }

    public async Task<List<OrgTypeFindAllDTO>> Handle(OrgTypeFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await OrgTypeService.Table.AsNoTracking()
            .ProjectTo<OrgTypeFindAllDTO>(OrgTypeService.mapper.ConfigurationProvider).ToListAsync();
    }
}



