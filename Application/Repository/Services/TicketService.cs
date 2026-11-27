
namespace Application.Repository.Services;

public class TicketService:BaseRepository<Ticket>
{
    public TicketService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

