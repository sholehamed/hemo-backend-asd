namespace Application.Core.Role_core.Queries;

public record RoleFindAllQuery : IRequest<List<RoleFindAllDTO>>
{
}

public record RoleFindAllDTO : IMapFrom<Role>
{
    [PropConfig("بالا دست")] public Guid? PRoleGUID { get; init; }

    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleFindAllDTO>();
    }
}

public class
    RoleFindAllQueryHandler : IRequestHandler<RoleFindAllQuery,
    List<RoleFindAllDTO>>
{
    private readonly RoleService RoleService;

    public RoleFindAllQueryHandler(RoleService RoleService)
    {
        this.RoleService = RoleService;
    }

    public async Task<List<RoleFindAllDTO>> Handle(RoleFindAllQuery request,
        CancellationToken cancellationToken)
    {
        return await RoleService.Table.AsNoTracking()
            .ProjectTo<RoleFindAllDTO>(RoleService.mapper.ConfigurationProvider).ToListAsync();
    }
}