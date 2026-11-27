namespace Application.Repository.Services;
public class HemoQualityLineService : BaseRepository<HemoQualityLine>
{
    public HemoQualityLineService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
