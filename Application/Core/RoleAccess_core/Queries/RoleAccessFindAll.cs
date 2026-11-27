

namespace Application.Core.RoleAccess_core.Queries;

public record RoleAccessFindAllQuery : IRequest<List<RoleAccessFindAllDTO>>
{

}

public record RoleAccessFindAllDTO : IMapFrom<RoleAccess>
{

    [PropConfig("منو")]
    public Guid MenuGUID { get; init; }

    [PropConfig("نقش")]
    public Guid RoleGUID { get; init; }

    [PropConfig("سرویس")]
    public Guid ApiGUID { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccess, RoleAccessFindAllDTO>();
    }
}

public class
    RoleAccessFindAllQueryHandler : IRequestHandler<RoleAccessFindAllQuery,
    List<RoleAccessFindAllDTO>>
{
    private RoleAccessService RoleAccessService;
    public RoleAccessFindAllQueryHandler(RoleAccessService RoleAccessService)
    {
        this.RoleAccessService = RoleAccessService;
    }

    public async Task<List<RoleAccessFindAllDTO>> Handle(RoleAccessFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await RoleAccessService.Table.AsNoTracking()
            .ProjectTo<RoleAccessFindAllDTO>(RoleAccessService.mapper.ConfigurationProvider).ToListAsync();
    }
}



