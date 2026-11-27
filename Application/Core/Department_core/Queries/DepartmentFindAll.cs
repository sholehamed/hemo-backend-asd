

namespace Application.Core.Department_core.Queries;

public record DepartmentFindAllQuery : IRequest<List<DepartmentFindAllDTO>>
{

}

public record DepartmentFindAllDTO : IMapFrom<Department>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public long Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentFindAllDTO>();
    }
}

public class
    DepartmentFindAllQueryHandler : IRequestHandler<DepartmentFindAllQuery,
    List<DepartmentFindAllDTO>>
{
    private DepartmentService DepartmentService;
    public DepartmentFindAllQueryHandler(DepartmentService DepartmentService)
    {
        this.DepartmentService = DepartmentService;
    }

    public async Task<List<DepartmentFindAllDTO>> Handle(DepartmentFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await DepartmentService.Table.AsNoTracking()
            .ProjectTo<DepartmentFindAllDTO>(DepartmentService.mapper.ConfigurationProvider).ToListAsync();
    }
}



