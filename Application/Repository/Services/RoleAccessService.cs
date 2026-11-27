
namespace Application.Repository.Services;

public class RoleAccessService:BaseRepository<RoleAccess>
{
    public RoleAccessService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

