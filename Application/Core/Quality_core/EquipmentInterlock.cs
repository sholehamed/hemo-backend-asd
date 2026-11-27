using Application.Common.Exceptions;
namespace Application.Core.Quality_core;
// Backward-compatible center/code association. An unregistered legacy code retains ST04 checks.
// Registered equipment must not be held, archived, or overdue for its recorded service.
public static class EquipmentInterlock
{
    public static async Task<bool> Available(IApplicationDbContext db,Guid org,string code,CancellationToken ct)
    {
        var e=await db.HemoEquipment.AsNoTracking().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.Code==code,ct);
        return e==null||QualityRules.CanOperate(e,DateTime.UtcNow);
    }
    public static async Task Require(IApplicationDbContext db,Guid org,string code,CancellationToken ct)
    {
        if(!await Available(db,org,code,ct))throw new ServiceExeption("تجهیز ثبت‌شده نیازمند بررسی فنی است؛ عملیات بانک خون مجاز نیست.");
    }
}
