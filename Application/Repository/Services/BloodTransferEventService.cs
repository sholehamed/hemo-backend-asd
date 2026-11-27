namespace Application.Repository.Services;
public class BloodTransferEventService : BaseRepository<BloodTransferEvent>
{
    public BloodTransferEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
