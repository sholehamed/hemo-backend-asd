namespace Application.Repository.Services;
public class HemoFacilityProfileService : BaseRepository<HemoFacilityProfile>
{
    public HemoFacilityProfileService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
