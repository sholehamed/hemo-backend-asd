using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Repository.Services;

public class OrgLocationService : BaseRepository<OrgLocation>
{
    private UserRoleService UserRoleService;

    public OrgLocationService(IApplicationDbContext context, IMapper mapper, UserRoleService userRoleService) : base(
        context, mapper)
    {
        UserRoleService = userRoleService;
    }

    public async Task<bool> OrgLocationIsValid(Guid? orgLocationGuid, bool CurrentOrgLocationIsValid = false)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        bool Valid = Table.Any(a =>
            a.GUID == orgLocationGuid &&
            a.Path.StartsWith(userRoleInfo.OrgLocationPath) &&
            (a.Path != userRoleInfo.OrgLocationPath || CurrentOrgLocationIsValid));
        if (!Valid)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است.");
        }
        else
        {
            return Valid;
        }
    }

    public async Task<bool> HasAccessToChild(string parentOrgLocationGUID, string childOrgLocationGUID)
    {
        Guid ParentOrgLocationGUID = Guid.Parse(parentOrgLocationGUID);
        Guid ChildOrgLocationGUID = Guid.Parse(childOrgLocationGUID);

        return await HasAccessToChild(ParentOrgLocationGUID, ChildOrgLocationGUID);
    }
    
    public async Task<bool> HasAccessToChild(Guid ParentOrgLocationGUID, Guid ChildOrgLocationGUID)
    {
        bool HasAccess = false;

        OrgLocation? ParentOrgLocation = db.OrgLocation.FirstOrDefault(a => a.GUID == ParentOrgLocationGUID);
        OrgLocation? ChildOrgLocation = db.OrgLocation.FirstOrDefault(a => a.GUID == ChildOrgLocationGUID);

        if (ParentOrgLocation != null && ChildOrgLocation != null && ChildOrgLocation.Path.StartsWith(ParentOrgLocation.Path))
        {
            HasAccess = true;
        }

        return HasAccess;
    }
    public async Task<List<Guid>> GetChilds(Guid ParentGuid)
    {
        OrgLocation? ParentOrgLocation = db.OrgLocation.FirstOrDefault(a => a.GUID == ParentGuid);
        List<Guid> Childs = new List<Guid>();
        if (ParentOrgLocation != null)
        {
            Childs = db.OrgLocation.Where(a => a.Path.StartsWith(ParentOrgLocation.Path)).Select(a => a.GUID).ToList();
        }
        return Childs;
    }

}