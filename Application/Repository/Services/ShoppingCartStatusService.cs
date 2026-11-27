
namespace Application.Repository.Services;

public class ShoppingCartStatusService:BaseRepository<ShoppingCartStatus>
{
    public ShoppingCartStatusService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

