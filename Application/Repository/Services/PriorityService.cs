
namespace Application.Repository.Services;

public class PriorityService:BaseRepository<Priority>
{
    public PriorityService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

