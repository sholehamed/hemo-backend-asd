namespace Application.Repository.Services;
public class BloodTransferVitalService : BaseRepository<BloodTransferVital>
{
    public BloodTransferVitalService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
