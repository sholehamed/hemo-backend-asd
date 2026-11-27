
namespace Application.Repository.Services;

public class DepartmentService:BaseRepository<Department>
{
    public DepartmentService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

