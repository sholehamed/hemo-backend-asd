namespace Application.Repository.Services;
public class PatientAdmissionReviewService : BaseRepository<PatientAdmissionReview>
{
    public PatientAdmissionReviewService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
