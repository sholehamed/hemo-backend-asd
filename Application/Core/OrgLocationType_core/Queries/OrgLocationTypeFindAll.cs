

namespace Application.Core.OrgLocationType_core.Queries;

public record OrgLocationTypeFindAllQuery : IRequest<List<OrgLocationTypeFindAllDTO>>
{

}

public record OrgLocationTypeFindAllDTO : IMapFrom<OrgLocationType>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocationType, OrgLocationTypeFindAllDTO>();
    }
}

public class
    OrgLocationTypeFindAllQueryHandler : IRequestHandler<OrgLocationTypeFindAllQuery,
    List<OrgLocationTypeFindAllDTO>>
{
    private OrgLocationTypeService OrgLocationTypeService;
    public OrgLocationTypeFindAllQueryHandler(OrgLocationTypeService OrgLocationTypeService)
    {
        this.OrgLocationTypeService = OrgLocationTypeService;
    }

    public async Task<List<OrgLocationTypeFindAllDTO>> Handle(OrgLocationTypeFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await OrgLocationTypeService.Table.AsNoTracking()
            .ProjectTo<OrgLocationTypeFindAllDTO>(OrgLocationTypeService.mapper.ConfigurationProvider).ToListAsync();
    }
}



