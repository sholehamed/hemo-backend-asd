namespace Application.Core.Role_core.Queries;

public record RoleFindByIdQuery(Guid GUID) : IRequest<RoleFindByIdDTO>;

public record RoleFindByIdDTO : IMapFrom<Role>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("نوع شرکت")] public Guid? OrgLocationTypeGUIO { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleFindByIdDTO>();
    }
}

public class
    RoleFindByIdQueryHandler : IRequestHandler<RoleFindByIdQuery, RoleFindByIdDTO>
{
    private readonly RoleService RoleService;

    public RoleFindByIdQueryHandler(RoleService RoleService)
    {
        this.RoleService = RoleService;
    }

    public async Task<RoleFindByIdDTO> Handle(RoleFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.GUID);
        var Data = RoleService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<RoleFindByIdDTO>(RoleService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}