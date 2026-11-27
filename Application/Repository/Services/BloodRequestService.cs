namespace Application.Repository.Services;
public class BloodRequestService : BaseRepository<BloodRequest>
{
    public BloodRequestService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
