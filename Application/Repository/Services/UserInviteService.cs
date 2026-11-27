
namespace Application.Repository.Services;

public class UserInviteService:BaseRepository<UserInvite>
{
    public UserInviteService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

