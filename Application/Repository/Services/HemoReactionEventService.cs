namespace Application.Repository.Services;
public class HemoReactionEventService : BaseRepository<HemoReactionEvent>
{
    public HemoReactionEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
