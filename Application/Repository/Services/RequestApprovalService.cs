namespace Application.Repository.Services;
public class RequestApprovalService : BaseRepository<RequestApproval>
{
    public RequestApprovalService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
