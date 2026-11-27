using Application.Common.Exceptions;

namespace Application.Repository.Services;

public class OrglocationPackageService : BaseRepository<OrglocationPackage>
{
    public OrglocationPackageService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<OrglocationPackage> GetValidOrglocationPackage(Guid OrglocationPackageGUID, Guid TargetOrglocationGUID)
    {
        DateTime Now = DateTime.Now;
        OrglocationPackage? orglocationPackage =
            db.OrglocationPackage.Include(a => a.Package)
                .FirstOrDefault(a =>
                    a.ExpiryDate > Now && a.OrgLocationGUID == TargetOrglocationGUID &&
                    a.GUID == OrglocationPackageGUID);

        if (orglocationPackage == null)
        {
            throw new ServiceExeption("اعتبار پکیح به اتمام رسیده است.");
        }

        return orglocationPackage;
    }
}