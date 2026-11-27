namespace Application.Repository.Services;
public class RequestClinicalDataService : BaseRepository<RequestClinicalData>
{
    public RequestClinicalDataService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
