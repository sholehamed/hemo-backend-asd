namespace Application.Repository.Services;
public class HemoQualityEventService : BaseRepository<HemoQualityEvent>
{
    public HemoQualityEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
