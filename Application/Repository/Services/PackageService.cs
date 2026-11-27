
namespace Application.Repository.Services;

public class PackageService:BaseRepository<Package>
{
    public PackageService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

