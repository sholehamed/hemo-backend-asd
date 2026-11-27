namespace Application.Repository.Services;
public class BankUnitEventService : BaseRepository<BankUnitEvent>
{
    public BankUnitEventService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
