
namespace Application.Repository.Services;

public class OTPService:BaseRepository<OTP>
{
    public OTPService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

