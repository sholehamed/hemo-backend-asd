
namespace Application.Repository.Services;

public class TicketMessageService:BaseRepository<TicketMessage>
{
    public TicketMessageService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

