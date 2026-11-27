namespace Application.Repository.Services;
public class HemoReactionService : BaseRepository<HemoReaction>
{
    public HemoReactionService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
