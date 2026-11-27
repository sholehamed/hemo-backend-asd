
using Application.Common.Models;

namespace Application.Core.DemoRequest_core.Queries;

public record DemoRequestWithPaginationQuery : PageFilter<DemoRequest, DemoRequestWithPaginationDTO>
{
    public bool? Accepted { get; init; }
    public Guid UserGUID { get; set; }
}

public record DemoRequestWithPaginationDTO : IMapFrom<DemoRequest>
{

    [PropConfig("نام")]
    public string UserFirstName { get; init; }
    
    [PropConfig("نام خانوادگی")]
    public string UserLastName { get; init; }
    
    [PropConfig("تلفن همراه")]
    public string UserMobile { get; init; }

    [PropConfig("پکیج")]
    public string Package { get; init; }

    [PropConfig("کد کاربر")]
    public Guid UserGUID { get; init; }
    
    [PropConfig(" شرکت")]
    public string OrgLocation { get; init; }

    [PropConfig("قبول شده")]
    public bool? Accepted { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    
    [PropConfig("تعداد درخواست قبلی")]
    public int TotalRequestCount { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<DemoRequest, DemoRequestWithPaginationDTO>()
            .ForMember(dest=>dest.UserFirstName,opt=>opt.MapFrom(src=>src.User.FirstName))
            .ForMember(dest=>dest.UserLastName,opt=>opt.MapFrom(src=>src.User.LastName))
            .ForMember(dest=>dest.UserMobile,opt=>opt.MapFrom(src=>src.User.Mobile))
            .ForMember(dest=>dest.Package,opt=>opt.MapFrom(src=>src.Package.Name))
            .ForMember(dest=>dest.OrgLocation,opt=>opt.MapFrom(src=>src.OrgLocation.Name))
            .ForMember(dest=>dest.TotalRequestCount,opt=>opt.MapFrom(src=>src.User.DemoRequests.Count))

            ;
    }
}

public class
    DemoRequestWithPaginationQueryHandler : IRequestHandler<DemoRequestWithPaginationQuery,
    PaginatedList<DemoRequestWithPaginationDTO>>
{
    private DemoRequestService DemoRequestService;
    public DemoRequestWithPaginationQueryHandler(DemoRequestService DemoRequestService)
    {
        this.DemoRequestService = DemoRequestService;
    }

    public async Task<PaginatedList<DemoRequestWithPaginationDTO>> Handle(DemoRequestWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<DemoRequest> queryable = DemoRequestService.Table.Where(a=>
            (request.UserGUID==Guid.Empty||a.UserGUID==request.UserGUID)&&(request.UserGUID!=Guid.Empty||a.Accepted==request.Accepted)
            );
        return await request.ToPaging( queryable,DemoRequestService.mapper);
    }
}

