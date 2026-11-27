
namespace Application.Repository.Services;

public class PackageCategoryService:BaseRepository<PackageCategory>
{
    public PackageCategoryService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

