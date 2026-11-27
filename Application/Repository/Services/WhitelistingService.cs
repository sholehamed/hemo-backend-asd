
namespace Application.Repository.Services;

public class WhitelistingService:BaseRepository<Whitelisting>
{
    public WhitelistingService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

