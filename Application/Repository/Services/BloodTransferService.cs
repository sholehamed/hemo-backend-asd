namespace Application.Repository.Services;
public class BloodTransferService : BaseRepository<BloodTransfer>
{
    public BloodTransferService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
