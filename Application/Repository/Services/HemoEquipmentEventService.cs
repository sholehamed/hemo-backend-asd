namespace Application.Repository.Services;
public class HemoEquipmentEventService : BaseRepository<HemoEquipmentEvent>
{
    public HemoEquipmentEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
