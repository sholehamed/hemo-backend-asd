namespace Application.Repository.Services;
public class BankBloodUnitService : BaseRepository<BankBloodUnit>
{
    public BankBloodUnitService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
