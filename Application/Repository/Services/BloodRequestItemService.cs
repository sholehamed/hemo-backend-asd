namespace Application.Repository.Services;
public class BloodRequestItemService : BaseRepository<BloodRequestItem>
{
    public BloodRequestItemService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
