namespace Application.Repository.Services;
public class HemoEquipmentIncidentService : BaseRepository<HemoEquipmentIncident>
{
    public HemoEquipmentIncidentService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
