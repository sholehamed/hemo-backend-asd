
namespace Application.Repository.Services;

public class OrgTypeService:BaseRepository<OrgType>
{
    public OrgTypeService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}

