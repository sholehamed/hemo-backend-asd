namespace Application.Repository.Services;
public class BankTemperatureLogService : BaseRepository<BankTemperatureLog>
{
    public BankTemperatureLogService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
