namespace Application.Repository.Services;
public class BankCompatibilityTestService : BaseRepository<BankCompatibilityTest>
{
    public BankCompatibilityTestService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
