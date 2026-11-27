
namespace Application.Repository.Services;

public class BlockListIPService:BaseRepository<BlockListIP>
{
    public BlockListIPService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

