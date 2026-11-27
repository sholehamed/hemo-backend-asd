
namespace Application.Repository.Services;

public class OrgLocationTypeService:BaseRepository<OrgLocationType>
{
    public OrgLocationTypeService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

