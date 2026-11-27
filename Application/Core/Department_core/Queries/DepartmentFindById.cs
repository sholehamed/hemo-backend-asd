
namespace Application.Core.Department_core.Queries;

public record DepartmentFindByIdQuery(Guid GUID) : IRequest<DepartmentFindByIdDTO>;

public record DepartmentFindByIdDTO : IMapFrom<Department>
{
    
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("کد")]
    public long Gkey { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentFindByIdDTO>();
    }
}

public class
    DepartmentFindByIdQueryHandler : IRequestHandler<DepartmentFindByIdQuery, DepartmentFindByIdDTO>
{
    private DepartmentService DepartmentService;

    public DepartmentFindByIdQueryHandler(DepartmentService DepartmentService)
    {
        this.DepartmentService = DepartmentService;
    }

    public async Task<DepartmentFindByIdDTO> Handle(DepartmentFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        DepartmentFindByIdDTO? Data = DepartmentService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<DepartmentFindByIdDTO>(DepartmentService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

