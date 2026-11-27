
namespace Application.Repository.Services;

public class MenuApiService:BaseRepository<MenuApi>
{
    public MenuApiService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

