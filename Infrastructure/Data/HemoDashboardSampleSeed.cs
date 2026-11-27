using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Data;

// Historical admissions only: no fabricated verified identities, transfusions, clinical findings or evidence.
internal static class HemoDashboardSampleSeed
{
    private const string Prefix="HEMO-DEMO-ST09:";
    public static async Task ApplyAsync(ApplicationDbContext db,Guid org,CancellationToken ct)
    {
        if(!await db.OrgLocation.AnyAsync(x=>x.GUID==org&&x.ConvertID=="HEMO-DEMO-ST02:Facility",ct))
            throw new InvalidOperationException("ST09 requires the dedicated training facility.");
        var actor=await db.UserRole.Include(x=>x.User).SingleAsync(x=>x.OrgLocationGUID==org&&x.User.Username=="hemo.admission",ct);
        var today=DateTime.UtcNow.Date;
        var offsets=new[]{1,2,2,3,4,5,7,8,10,13};
        for(var i=0;i<offsets.Length;i++)
        {
            var key=(i+1).ToString("D2");var his="DEMO-ST09-"+key;
            var existing=await db.PatientAdmission.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.HISNumber==his,ct);
            if(existing!=null)
            {
                if(existing.ConvertID!=Prefix+"admission:"+key)throw new InvalidOperationException("Conflicting ST09 training admission.");
                continue;
            }
            var date=today.AddDays(-offsets[i]).AddHours(10);
            var p=new Patient{OrgLocationGUID=org,IdentificationType="Unknown",IdentificationNumber="DEMO-ST09-ID-"+key,FirstName="بیمار ساختگی",LastName="آمار آموزشی "+key,Gender="Unknown",ConvertID=Prefix+"patient:"+key,Created=date,CreatedBy=actor.UserGUID,CreatedByIP="demo",LastModified=date,LastModifiedBy=actor.UserGUID,LastModifiedByIP="demo"};
            var a=new PatientAdmission{PatientGUID=p.GUID,OrgLocationGUID=org,HISNumber=his,WristbandCode="DEMO-ST09-WRIST-"+key,Ward=i%2==0?"بخش آموزشی داخلی":"بخش آموزشی جراحی",BedNumber="DEMO-"+key,Physician="پزشک آموزشی",AdmissionType="Inpatient",Status="Active",IdentityStatus="Pending",AdmittedAt=date,Notes="داده ساختگی آموزشی ST-09؛ فاقد تأیید هویت یا شواهد بالینی",ConvertID=Prefix+"admission:"+key,Created=date,CreatedBy=actor.UserGUID,CreatedByIP="demo",LastModified=date,LastModifiedBy=actor.UserGUID,LastModifiedByIP="demo"};
            db.Patient.Add(p);db.PatientAdmission.Add(a);
        }
        await db.SaveChangesAsync(ct);
    }
}
