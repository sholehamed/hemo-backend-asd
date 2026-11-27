using Application.Common.Models;

namespace Application.Core.SubSystem_core.Queries;

public record SubSystemWithPaginationQuery : PageFilter<SubSystem, SubSystemWithPaginationDTO>
{
}

public record SubSystemWithPaginationDTO : IMapFrom<SubSystem>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("آدرس سرویس")] public string Host { get; init; }

    [PropConfig("پورت سرویس")] public int Port { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("فعال")] public bool Active { get; init; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SubSystem, SubSystemWithPaginationDTO>();
    }
}

public class
    SubSystemWithPaginationQueryHandler : IRequestHandler<SubSystemWithPaginationQuery,
    PaginatedList<SubSystemWithPaginationDTO>>
{
    private readonly SubSystemService SubSystemService;

    public SubSystemWithPaginationQueryHandler(SubSystemService SubSystemService)
    {
        this.SubSystemService = SubSystemService;
    }

    public async Task<PaginatedList<SubSystemWithPaginationDTO>> Handle(SubSystemWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<SubSystem> queryable = SubSystemService.Table;
        return await request.ToPaging(queryable, SubSystemService.mapper);
    }
}