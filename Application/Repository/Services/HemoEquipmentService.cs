namespace Application.Repository.Services;
public class HemoEquipmentService : BaseRepository<HemoEquipment>
{
    public HemoEquipmentService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
