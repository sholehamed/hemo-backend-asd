
namespace Application.Repository.Services;

public class PasswordBlockListService:BaseRepository<PasswordBlockList>
{
    public PasswordBlockListService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

