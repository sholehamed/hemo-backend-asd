namespace Application.Repository.Services;
public class HemoReportService : BaseRepository<HemoReport>
{
    public HemoReportService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
