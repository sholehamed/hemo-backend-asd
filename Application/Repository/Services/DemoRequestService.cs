
namespace Application.Repository.Services;

public class DemoRequestService:BaseRepository<DemoRequest>
{
    public DemoRequestService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

