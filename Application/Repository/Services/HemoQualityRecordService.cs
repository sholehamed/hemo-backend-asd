namespace Application.Repository.Services;
public class HemoQualityRecordService : BaseRepository<HemoQualityRecord>
{
    public HemoQualityRecordService(IApplicationDbContext context,IMapper mapper):base(context,mapper){}
}
