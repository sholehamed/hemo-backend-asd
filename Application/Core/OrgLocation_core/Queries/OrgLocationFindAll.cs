

namespace Application.Core.OrgLocation_core.Queries;

public record OrgLocationFindAllQuery : IRequest<List<OrgLocationFindAllDTO>>
{

}

public record OrgLocationFindAllDTO : IMapFrom<OrgLocation>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("شرکت پدر")]
    public Guid? POrgLocationGUID { get; init; }

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
        profile.CreateMap<OrgLocation, OrgLocationFindAllDTO>();
    }
}

public class
    OrgLocationFindAllQueryHandler : IRequestHandler<OrgLocationFindAllQuery,
    List<OrgLocationFindAllDTO>>
{
    private OrgLocationService OrgLocationService;
    public OrgLocationFindAllQueryHandler(OrgLocationService OrgLocationService)
    {
        this.OrgLocationService = OrgLocationService;
    }

    public async Task<List<OrgLocationFindAllDTO>> Handle(OrgLocationFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await OrgLocationService.Table.AsNoTracking()
            .ProjectTo<OrgLocationFindAllDTO>(OrgLocationService.mapper.ConfigurationProvider).ToListAsync();
    }
}



