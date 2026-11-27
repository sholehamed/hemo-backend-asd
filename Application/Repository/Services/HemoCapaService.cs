namespace Application.Repository.Services;
public class HemoCapaService : BaseRepository<HemoCapa>
{
    public HemoCapaService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
