
namespace Application.Repository.Services;

public class ssoService:BaseRepository<sso>
{
    public ssoService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

