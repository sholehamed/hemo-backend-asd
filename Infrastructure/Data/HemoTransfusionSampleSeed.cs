using System.Text.Json;
using Application.Core.BloodRequest_core;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Data;

// Explicit educational fixture only. Called inside the opt-in Development HemoDemoSeed transaction.
// Never refresh the expiry or overwrite an exercised case. No real identifiers or clinical evidence.
internal static class HemoTransfusionSampleSeed
{
    private const string Prefix="HEMO-DEMO-ST05:";
    private const string Note="داده ساختگی آموزشی ST-05 — فقط برای تمرین نرم‌افزار؛ نه گزارش یا مجوز بالینی";
    private static T Mark<T>(T row,string key,Guid actor,DateTime now) where T:AuditedEntity
    {row.ConvertID=Prefix+key;row.Created=now;row.CreatedBy=actor;row.CreatedByIP="demo";row.LastModified=now;row.LastModifiedBy=actor;row.LastModifiedByIP="demo";return row;}
    public static async Task ApplyAsync(ApplicationDbContext db,Guid org,CancellationToken ct)
    {
        if(!await db.OrgLocation.AnyAsync(x=>x.GUID==org&&x.ConvertID=="HEMO-DEMO-ST02:Facility",ct))throw new InvalidOperationException("ST05 fixtures require the dedicated demo facility.");
        async Task<UserRole> Actor(string username)=>await db.UserRole.Include(x=>x.User).SingleAsync(x=>x.OrgLocationGUID==org&&x.User.Username==username,ct);
        var nurse=await Actor("hemo.nurse");var doctor=await Actor("hemo.doctor");var bank=await Actor("hemo.bloodbank");var supervisor=await Actor("hemo.bank.supervisor");var admissionActor=await Actor("hemo.admission");
        var now=DateTime.UtcNow;
        var cases=new[]{(Key:"01",Product:"RBC",Name:"تکمیل تزریق",Ward:"بخش آموزشی داخلی",Equipment:"DEMO-ST05-FRIDGE"),
                        (Key:"02",Product:"RBC",Name:"مرجوعی پیش از شروع",Ward:"بخش آموزشی جراحی",Equipment:"DEMO-ST05-FRIDGE"),
                        (Key:"03",Product:"RBC",Name:"توقف و نگرانی",Ward:"بخش آموزشی اورژانس",Equipment:"DEMO-ST05-FRIDGE"),
                        (Key:"04",Product:"FFP",Name:"تمرین فرآورده ذوب‌شده",Ward:"بخش آموزشی داخلی",Equipment:"DEMO-ST05-FFP")};
        // Each case has a complete pre-existing synthetic history up to Ready.
        foreach(var f in cases)
        {
            var his="DEMO-ST05-"+f.Key;
            var old=await db.PatientAdmission.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.HISNumber==his,ct);
            if(old!=null)
            {
                if(old.ConvertID!=Prefix+"admission:"+f.Key)throw new InvalidOperationException("Conflicting ST05 training case.");
                continue;
            }
            var p=Mark(new Patient{OrgLocationGUID=org,IdentificationType="Unknown",IdentificationNumber="DEMO-ST05-ID-"+f.Key,FirstName="بیمار ساختگی",LastName=f.Name,Gender="Unknown"}, "patient:"+f.Key,admissionActor.UserGUID,now);
            var a=Mark(new PatientAdmission{PatientGUID=p.GUID,OrgLocationGUID=org,HISNumber=his,WristbandCode="DEMO-WRIST-"+f.Key,Ward=f.Ward,BedNumber="DEMO-"+f.Key,Physician="پزشک آموزشی",AdmissionType="Inpatient",Status="Active",IdentityStatus="Verified",IdentityReviewedAt=now.AddMinutes(-20),AdmittedAt=now.AddHours(-1),Notes=Note}, "admission:"+f.Key,admissionActor.UserGUID,now);
            db.Patient.Add(p);db.PatientAdmission.Add(a);await db.SaveChangesAsync(ct);
            var review=Mark(new PatientAdmissionReview{PatientAdmissionGUID=a.GUID,Kind="Identity",Outcome="Verified",Checklist=31,Notes=Note,ReviewedAt=now.AddMinutes(-20)}, "review:"+f.Key,admissionActor.UserGUID,now);
            db.PatientAdmissionReview.Add(review);
            var r=Mark(new BloodRequest{PatientAdmissionGUID=a.GUID,ClientRequestId=Guid.NewGuid(),RequestNumber="DEMO-BR-ST05-"+f.Key,RequesterUserGUID=nurse.UserGUID,RequesterRoleGUID=nurse.RoleGUID,Source="Nurse",FormType="General",Priority="Routine",Status="AwaitingBank",SubmissionAttempt=1,SubmittedAt=now.AddMinutes(-15),AdmissionVersionAtSubmission=a.Version.ToArray()}, "request:"+f.Key,nurse.UserGUID,now);
            db.BloodRequest.Add(r);
            db.BloodRequestItem.Add(Mark(new BloodRequestItem{BloodRequestGUID=r.GUID,ProductCode=f.Product,Quantity=1,SpecialInstructions=Note}, "line:"+f.Key,nurse.UserGUID,now));
            db.RequestClinicalData.Add(Mark(new RequestClinicalData{BloodRequestGUID=r.GUID,ClinicalIndication=Note,Diagnosis="آموزشی"}, "clinical:"+f.Key,nurse.UserGUID,now));
            var snapshot=JsonSerializer.Serialize(new{r.FormType,r.Priority,Physician=new PhysicianCompletion("آموزشی",Note,"DEMO-ONLY",Note,true),AdmissionVersion=r.AdmissionVersionAtSubmission});
            db.RequestApproval.Add(Mark(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=nurse.UserGUID,ActorRoleGUID=nurse.RoleGUID,Action="Submit",PreviousStatus="Draft",NewStatus="AwaitingDoctor",SubmissionAttempt=1,OccurredAt=now.AddMinutes(-15),Reason=Note,SnapshotJson=snapshot},"submit:"+f.Key,nurse.UserGUID,now));
            db.RequestApproval.Add(Mark(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=doctor.UserGUID,ActorRoleGUID=doctor.RoleGUID,Action="Approve",PreviousStatus="AwaitingDoctor",NewStatus="AwaitingBank",SubmissionAttempt=1,OccurredAt=now.AddMinutes(-14),Reason=Note,SnapshotJson=snapshot},"approve:"+f.Key,doctor.UserGUID,now));
            await db.SaveChangesAsync(ct);
            var u=Mark(new BankBloodUnit{OrgLocationGUID=org,UnitCode="DEMO-UNIT-"+f.Key,ProductCode=f.Product,BloodGroup="O+",BatchNumber="DEMO-ST05",SupplySource=Note,EquipmentCode=f.Equipment,ReceivedAt=now.AddHours(-1),ExpiresAt=now.AddDays(7),Status="Ready",InspectionAt=now.AddMinutes(-12),BloodRequestGUID=r.GUID,ThawStartedAt=f.Product=="FFP"?now.AddMinutes(-8):null,ThawCompletedAt=f.Product=="FFP"?now.AddMinutes(-5):null},"unit:"+f.Key,bank.UserGUID,now);
            db.BankBloodUnit.Add(u);
            db.BankCompatibilityTest.Add(Mark(new BankCompatibilityTest{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,SubmissionAttempt=1,RequestVersion=r.Version.ToArray(),AdmissionVersion=a.Version.ToArray(),ActorUserGUID=bank.UserGUID,ActorRoleGUID=bank.RoleGUID,RecordedAt=now.AddMinutes(-10),SampleReference="DEMO-SAMPLE-"+f.Key,SecondSample="Confirmed",LabelGroup=u.BloodGroup,Method="آموزشی",ImmediateSpin="NonReactive",Phase37="NonReactive",Ahg="NonReactive",Dat="NonReactive",AntibodyScreen="NonReactive",ReportReference="DEMO-REPORT-"+f.Key,Conclusion="Accepted",Notes=Note},"lab:"+f.Key,bank.UserGUID,now));
            var movements=new List<(string Action,string Before,string After,UserRole Actor,int Minutes)>{("Receive","","ReceivedHold",bank,13),("Inspect","ReceivedHold","Available",supervisor,12),("Test","Available","Available",bank,10),("Reserve","Available","Reserved",bank,9)};
            if(f.Product=="FFP"){movements.Add(("StartThaw","Reserved","Thawing",bank,8));movements.Add(("FinishThaw","Thawing","Reserved",bank,5));}
            movements.Add(("Ready","Reserved","Ready",bank,4));
            foreach(var m in movements)db.BankUnitEvent.Add(Mark(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,ActorUserGUID=m.Actor.UserGUID,ActorRoleGUID=m.Actor.RoleGUID,Action=m.Action,PreviousStatus=m.Before,NewStatus=m.After,OccurredAt=now.AddMinutes(-m.Minutes),Reason=Note},"event:"+f.Key+":"+m.Action,m.Actor.UserGUID,now));
            if(!await db.BankTemperatureLog.AnyAsync(t=>t.OrgLocationGUID==org&&t.EquipmentCode==f.Equipment,ct))
                db.BankTemperatureLog.Add(Mark(new BankTemperatureLog{OrgLocationGUID=org,EquipmentCode=f.Equipment,MeasuredAt=now.AddMinutes(-16),RecordedAt=now.AddMinutes(-16),Temperature=0m,Assessment="Acceptable",Notes=Note,ActorUserGUID=bank.UserGUID,ActorRoleGUID=bank.RoleGUID},"temperature:"+f.Equipment,bank.UserGUID,now));
            await db.SaveChangesAsync(ct);
        }
    }
}
