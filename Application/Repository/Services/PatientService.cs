namespace Application.Repository.Services;
public class PatientService : BaseRepository<Patient>
{
    public PatientService(IApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
}
