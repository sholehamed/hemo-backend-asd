

namespace Application.Core.DemoRequest_core.Queries;

public record DemoRequestFindAllQuery : IRequest<List<DemoRequestFindAllDTO>>
{

}

public record DemoRequestFindAllDTO : IMapFrom<DemoRequest>
{

    [PropConfig("کاربر ثبت کننده")]
    public Guid UserGUID { get; init; }

    [PropConfig("پکیج")]
    public Guid PackageGUID { get; init; }

    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }

    [PropConfig("قبول شده")]
    public bool? Accepted { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    [PropConfig("کد کانورت دیتا")]
    public string? ConvertID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<DemoRequest, DemoRequestFindAllDTO>();
    }
}

public class
    DemoRequestFindAllQueryHandler : IRequestHandler<DemoRequestFindAllQuery,
    List<DemoRequestFindAllDTO>>
{
    private DemoRequestService DemoRequestService;
    public DemoRequestFindAllQueryHandler(DemoRequestService DemoRequestService)
    {
        this.DemoRequestService = DemoRequestService;
    }

    public async Task<List<DemoRequestFindAllDTO>> Handle(DemoRequestFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await DemoRequestService.Table.AsNoTracking()
            .ProjectTo<DemoRequestFindAllDTO>(DemoRequestService.mapper.ConfigurationProvider).ToListAsync();
    }
}



