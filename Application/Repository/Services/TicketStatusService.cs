
namespace Application.Repository.Services;

public class TicketStatusService:BaseRepository<TicketStatus>
{
    public TicketStatusService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

