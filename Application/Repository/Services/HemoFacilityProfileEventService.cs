namespace Application.Repository.Services;
public class HemoFacilityProfileEventService : BaseRepository<HemoFacilityProfileEvent>
{
    public HemoFacilityProfileEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
