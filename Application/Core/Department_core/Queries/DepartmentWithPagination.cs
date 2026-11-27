
using Application.Common.Models;

namespace Application.Core.Department_core.Queries;

public record DepartmentWithPaginationQuery : PageFilter<Department, DepartmentWithPaginationDTO>
{

}

public record DepartmentWithPaginationDTO : IMapFrom<Department>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public long Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentWithPaginationDTO>();
    }
}

public class
    DepartmentWithPaginationQueryHandler : IRequestHandler<DepartmentWithPaginationQuery,
    PaginatedList<DepartmentWithPaginationDTO>>
{
    private DepartmentService DepartmentService;
    public DepartmentWithPaginationQueryHandler(DepartmentService DepartmentService)
    {
        this.DepartmentService = DepartmentService;
    }

    public async Task<PaginatedList<DepartmentWithPaginationDTO>> Handle(DepartmentWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Department> queryable = DepartmentService.Table;
        return await request.ToPaging( queryable,DepartmentService.mapper);
    }
}

