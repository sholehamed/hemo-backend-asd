namespace Application.Repository.Services;
public class PatientAdmissionService : BaseRepository<PatientAdmission>
{
    public PatientAdmissionService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
