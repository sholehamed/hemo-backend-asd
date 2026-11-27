
namespace Application.Repository.Services;

public class ShoppingCartProductService:BaseRepository<ShoppingCartProduct>
{
    public ShoppingCartProductService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

