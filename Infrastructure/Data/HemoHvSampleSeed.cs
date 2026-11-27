using System.Text.Json;
using Application.Core.BloodRequest_core;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Data;

// Explicit educational fixture only. Called inside the opt-in Development HemoDemoSeed transaction.
// Never refresh the expiry or overwrite an exercised case. No real identifiers or clinical evidence.
internal static class HemoHvSampleSeed
{
    private const string Prefix="HEMO-DEMO-ST06:";
    private const string Note="داده ساختگی آموزشی ST-05 — فقط برای تمرین نرم‌افزار؛ نه گزارش یا مجوز بالینی";
    private static T Mark<T>(T row,string key,Guid actor,DateTime now) where T:AuditedEntity
    {row.ConvertID=Prefix+key;row.Created=now;row.CreatedBy=actor;row.CreatedByIP="demo";row.LastModified=now;row.LastModifiedBy=actor;row.LastModifiedByIP="demo";return row;}
    public static async Task ApplyAsync(ApplicationDbContext db,Guid org,CancellationToken ct)
    {
        if(!await db.OrgLocation.AnyAsync(x=>x.GUID==org&&x.ConvertID=="HEMO-DEMO-ST02:Facility",ct))throw new InvalidOperationException("ST06 fixtures require the dedicated demo facility.");
        async Task<UserRole> Actor(string username)=>await db.UserRole.Include(x=>x.User).SingleAsync(x=>x.OrgLocationGUID==org&&x.User.Username==username,ct);
        var nurse=await Actor("hemo.nurse");var doctor=await Actor("hemo.doctor");var bank=await Actor("hemo.bloodbank");var supervisor=await Actor("hemo.bank.supervisor");var admissionActor=await Actor("hemo.admission");
        var witness=await Actor("hemo.nurse.witness");var officer=await Actor("hemo.hv.officer");
        var now=DateTime.UtcNow.AddMinutes(-30);
        var cases=new[]{(Key:"01",Product:"RBC",Name:"ثبت گزارش اولیه",Ward:"بخش آموزشی داخلی",Equipment:"DEMO-ST06-FRIDGE"),
                        (Key:"02",Product:"RBC",Name:"بررسی پزشک",Ward:"بخش آموزشی جراحی",Equipment:"DEMO-ST06-FRIDGE"),
                        (Key:"03",Product:"RBC",Name:"تصمیم RCA",Ward:"بخش آموزشی اورژانس",Equipment:"DEMO-ST06-FRIDGE"),
                        (Key:"04",Product:"FFP",Name:"پیگیری CAPA",Ward:"بخش آموزشی داخلی",Equipment:"DEMO-ST06-FFP")};
        // Each case has a complete pre-existing synthetic history up to Ready.
        foreach(var f in cases)
        {
            var his="DEMO-ST06-"+f.Key;
            var old=await db.PatientAdmission.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.HISNumber==his,ct);
            if(old!=null)
            {
                if(old.ConvertID!=Prefix+"admission:"+f.Key)throw new InvalidOperationException("Conflicting ST06 training case.");
                continue;
            }
            var p=Mark(new Patient{OrgLocationGUID=org,IdentificationType="Unknown",IdentificationNumber="DEMO-ST06-ID-"+f.Key,FirstName="بیمار ساختگی",LastName=f.Name,Gender="Unknown"}, "patient:"+f.Key,admissionActor.UserGUID,now);
            var a=Mark(new PatientAdmission{PatientGUID=p.GUID,OrgLocationGUID=org,HISNumber=his,WristbandCode="DEMO-HV-WRIST-"+f.Key,Ward=f.Ward,BedNumber="DEMO-"+f.Key,Physician="پزشک آموزشی",AdmissionType="Inpatient",Status="Active",IdentityStatus="Verified",IdentityReviewedAt=now.AddMinutes(-20),AdmittedAt=now.AddHours(-1),Notes=Note}, "admission:"+f.Key,admissionActor.UserGUID,now);
            db.Patient.Add(p);db.PatientAdmission.Add(a);await db.SaveChangesAsync(ct);
            var review=Mark(new PatientAdmissionReview{PatientAdmissionGUID=a.GUID,Kind="Identity",Outcome="Verified",Checklist=31,Notes=Note,ReviewedAt=now.AddMinutes(-20)}, "review:"+f.Key,admissionActor.UserGUID,now);
            db.PatientAdmissionReview.Add(review);
            var r=Mark(new BloodRequest{PatientAdmissionGUID=a.GUID,ClientRequestId=Guid.NewGuid(),RequestNumber="DEMO-BR-ST06-"+f.Key,RequesterUserGUID=nurse.UserGUID,RequesterRoleGUID=nurse.RoleGUID,Source="Nurse",FormType="General",Priority="Routine",Status="AwaitingBank",SubmissionAttempt=1,SubmittedAt=now.AddMinutes(-15),AdmissionVersionAtSubmission=a.Version.ToArray()}, "request:"+f.Key,nurse.UserGUID,now);
            db.BloodRequest.Add(r);
            db.BloodRequestItem.Add(Mark(new BloodRequestItem{BloodRequestGUID=r.GUID,ProductCode=f.Product,Quantity=1,SpecialInstructions=Note}, "line:"+f.Key,nurse.UserGUID,now));
            db.RequestClinicalData.Add(Mark(new RequestClinicalData{BloodRequestGUID=r.GUID,ClinicalIndication=Note,Diagnosis="آموزشی"}, "clinical:"+f.Key,nurse.UserGUID,now));
            var snapshot=JsonSerializer.Serialize(new{r.FormType,r.Priority,Physician=new PhysicianCompletion("آموزشی",Note,"DEMO-ONLY",Note,true),AdmissionVersion=r.AdmissionVersionAtSubmission});
            db.RequestApproval.Add(Mark(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=nurse.UserGUID,ActorRoleGUID=nurse.RoleGUID,Action="Submit",PreviousStatus="Draft",NewStatus="AwaitingDoctor",SubmissionAttempt=1,OccurredAt=now.AddMinutes(-15),Reason=Note,SnapshotJson=snapshot},"submit:"+f.Key,nurse.UserGUID,now));
            db.RequestApproval.Add(Mark(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=doctor.UserGUID,ActorRoleGUID=doctor.RoleGUID,Action="Approve",PreviousStatus="AwaitingDoctor",NewStatus="AwaitingBank",SubmissionAttempt=1,OccurredAt=now.AddMinutes(-14),Reason=Note,SnapshotJson=snapshot},"approve:"+f.Key,doctor.UserGUID,now));
            await db.SaveChangesAsync(ct);
            var u=Mark(new BankBloodUnit{OrgLocationGUID=org,UnitCode="DEMO-HV-UNIT-"+f.Key,ProductCode=f.Product,BloodGroup="O+",BatchNumber="DEMO-ST06",SupplySource=Note,EquipmentCode=f.Equipment,ReceivedAt=now.AddHours(-1),ExpiresAt=now.AddDays(7),Status="Ready",InspectionAt=now.AddMinutes(-12),BloodRequestGUID=r.GUID,ThawStartedAt=f.Product=="FFP"?now.AddMinutes(-8):null,ThawCompletedAt=f.Product=="FFP"?now.AddMinutes(-5):null},"unit:"+f.Key,bank.UserGUID,now);
            db.BankBloodUnit.Add(u);
            var lab=Mark(new BankCompatibilityTest{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,SubmissionAttempt=1,RequestVersion=r.Version.ToArray(),AdmissionVersion=a.Version.ToArray(),ActorUserGUID=bank.UserGUID,ActorRoleGUID=bank.RoleGUID,RecordedAt=now.AddMinutes(-10),SampleReference="DEMO-HV-SAMPLE-"+f.Key,SecondSample="Confirmed",LabelGroup=u.BloodGroup,Method="آموزشی",ImmediateSpin="NonReactive",Phase37="NonReactive",Ahg="NonReactive",Dat="NonReactive",AntibodyScreen="NonReactive",ReportReference="DEMO-HV-REPORT-"+f.Key,Conclusion="Accepted",Notes=Note},"lab:"+f.Key,bank.UserGUID,now);db.BankCompatibilityTest.Add(lab);
            var movements=new List<(string Action,string Before,string After,UserRole Actor,int Minutes)>{("Receive","","ReceivedHold",bank,13),("Inspect","ReceivedHold","Available",supervisor,12),("Test","Available","Available",bank,10),("Reserve","Available","Reserved",bank,9)};
            if(f.Product=="FFP"){movements.Add(("StartThaw","Reserved","Thawing",bank,8));movements.Add(("FinishThaw","Thawing","Reserved",bank,5));}
            movements.Add(("Ready","Reserved","Ready",bank,4));
            foreach(var m in movements)db.BankUnitEvent.Add(Mark(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,ActorUserGUID=m.Actor.UserGUID,ActorRoleGUID=m.Actor.RoleGUID,Action=m.Action,PreviousStatus=m.Before,NewStatus=m.After,OccurredAt=now.AddMinutes(-m.Minutes),Reason=Note},"event:"+f.Key+":"+m.Action,m.Actor.UserGUID,now));
            if(!await db.BankTemperatureLog.AnyAsync(t=>t.OrgLocationGUID==org&&t.EquipmentCode==f.Equipment,ct))
                db.BankTemperatureLog.Add(Mark(new BankTemperatureLog{OrgLocationGUID=org,EquipmentCode=f.Equipment,MeasuredAt=now.AddMinutes(-16),RecordedAt=now.AddMinutes(-16),Temperature=0m,Assessment="Acceptable",Notes=Note,ActorUserGUID=bank.UserGUID,ActorRoleGUID=bank.RoleGUID},"temperature:"+f.Equipment,bank.UserGUID,now));
            await db.SaveChangesAsync(ct);
            // These are independent synthetic ST06 prerequisites, never modify existing ST05 cases.
            var t=Mark(new BloodTransfer{OrgLocationGUID=org,BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,PatientAdmissionGUID=a.GUID,BankCompatibilityTestGUID=lab.GUID,
                TransferNumber="DEMO-TX-ST06-"+f.Key,PatientName=p.FirstName+" "+p.LastName,HisNumber=a.HISNumber,Ward=a.Ward,WristbandCode=a.WristbandCode,UnitCode=u.UnitCode,ProductCode=u.ProductCode,BloodGroup=u.BloodGroup,
                RequestVersion=r.Version.ToArray(),AdmissionVersion=a.Version.ToArray(),Status="Stopped",DispatchedAt=now,DispatchedBy=supervisor.UserGUID,CarrierName="حامل آموزشی",CarrierCode="DEMO",ContainerCode="DEMO-HV",TransportIndicator="DEMO-ONLY",ReceivedAt=now.AddMinutes(1),ReceivedBy=nurse.UserGUID,VerifiedAt=now.AddMinutes(2),VerifiedBy=nurse.UserGUID,WitnessedAt=now.AddMinutes(3),WitnessedBy=witness.UserGUID,StartedAt=now.AddMinutes(5),StartedBy=nurse.UserGUID,EndedAt=now.AddMinutes(8),EndedBy=nurse.UserGUID,SuspectedReaction=true},"transfer:"+f.Key,nurse.UserGUID,now);
            db.BloodTransfer.Add(t);u.Status="TransfusionStopped";
            var history=new[]{("Dispatch","","Dispatched",supervisor,0),("Receive","Dispatched","Received",nurse,1),("Verify","Received","Verified",nurse,2),("Witness","Verified","Witnessed",witness,3),("Vital","Witnessed","Witnessed",nurse,4),("Start","Witnessed","InProgress",nurse,5),("Stop","InProgress","Stopped",nurse,8)};
            foreach(var e in history)db.BloodTransferEvent.Add(Mark(new BloodTransferEvent{BloodTransferGUID=t.GUID,Action=e.Item1,PreviousStatus=e.Item2,NewStatus=e.Item3,ActorUserGUID=e.Item4.UserGUID,ActorRoleGUID=e.Item4.RoleGUID,OccurredAt=now.AddMinutes(e.Item5),Notes=Note},"txevent:"+f.Key+":"+e.Item1,e.Item4.UserGUID,now));
            db.BankUnitEvent.Add(Mark(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,ActorUserGUID=supervisor.UserGUID,ActorRoleGUID=supervisor.RoleGUID,Action="Dispatch",PreviousStatus="Ready",NewStatus="Issued",OccurredAt=now,Reason=Note},"issue:"+f.Key,supervisor.UserGUID,now));
            db.BankUnitEvent.Add(Mark(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,ActorUserGUID=nurse.UserGUID,ActorRoleGUID=nurse.RoleGUID,Action="Stop",PreviousStatus="Issued",NewStatus="TransfusionStopped",OccurredAt=now.AddMinutes(8),Reason=Note},"stop:"+f.Key,nurse.UserGUID,now));
            db.BloodTransferVital.Add(Mark(new BloodTransferVital{BloodTransferGUID=t.GUID,ActorUserGUID=nurse.UserGUID,ActorRoleGUID=nurse.RoleGUID,Phase="T0",MeasuredAt=now.AddMinutes(4),RecordedAt=now.AddMinutes(4),Notes=Note},"vital:"+f.Key,nurse.UserGUID,now));
            if(f.Key!="01")
            {
                var hv=Mark(new HemoReaction{OrgLocationGUID=org,BloodTransferGUID=t.GUID,ClientReportId=Guid.NewGuid(),CaseNumber="DEMO-HV-"+f.Key,Status="Reported",OnsetAt=now.AddMinutes(7),ReportedAt=now.AddMinutes(9),ReporterUserGUID=nurse.UserGUID,SymptomsJson=JsonSerializer.Serialize(new[]{"Unknown"}),InitialSeverity="Unknown",InitialNotes=Note,ActionsTaken=Note,SamplesSent=Note},"case:"+f.Key,nurse.UserGUID,now);
                db.HemoReaction.Add(hv);
                void E(string action,string previous,string next,UserRole actor,int minute){db.HemoReactionEvent.Add(Mark(new HemoReactionEvent{HemoReactionGUID=hv.GUID,ActorUserGUID=actor.UserGUID,ActorRoleGUID=actor.RoleGUID,Action=action,PreviousStatus=previous,NewStatus=next,OccurredAt=now.AddMinutes(minute),Notes=Note},"hvevent:"+f.Key+":"+action,actor.UserGUID,now));}
                E("Report","","Reported",nurse,9);
                if(f.Key is "03" or "04")
                {
                    hv.Status="BankReviewed";hv.DoctorUserGUID=doctor.UserGUID;hv.DoctorReviewedAt=now.AddMinutes(10);hv.DiagnosisCode="Undetermined";hv.Severity="Unknown";hv.Imputability="Unknown";hv.Outcome="Unknown";hv.DoctorNotes=Note;hv.DoctorActions=Note;hv.DoctorReference="DEMO-ONLY";
                    E("DoctorReview","Reported","DoctorReviewed",doctor,10);
                    hv.BankUserGUID=bank.UserGUID;hv.BankReviewedAt=now.AddMinutes(11);hv.BankFindings=Note;hv.BankSampleReference="DEMO-ONLY";hv.BankReportReference="DEMO-ONLY";hv.BankDisposition=Note;
                    E("BankReview","DoctorReviewed","BankReviewed",bank,11);
                }
                if(f.Key=="04")
                {
                    hv.Status="CapaMonitoring";hv.InvestigationNotes=Note;hv.RcaRequired=true;hv.ExternalStatus="NotRequired";hv.RcaLead="رهبر آموزشی";hv.RcaMethod="FiveWhys";hv.RcaMeetingAt=now.AddMinutes(12);hv.RcaParticipants=Note;hv.DirectCause=Note;hv.RootCause=Note;hv.RcaPublishedAt=now.AddMinutes(14);hv.RcaPublishedBy=officer.UserGUID;
                    E("Investigate","BankReviewed","RcaOpen",officer,12);
                    var c=Mark(new HemoCapa{HemoReactionGUID=hv.GUID,Title="تمرین بازبینی گردش ثبت گزارش",Kind="Corrective",OwnerUserGUID=nurse.UserGUID,OwnerName=nurse.User.FirstName+" "+nurse.User.LastName,DueAt=now.AddDays(7),Indicator="ثبت مستند نتیجه تمرین",Status="Open"},"capa:"+f.Key,nurse.UserGUID,now);
                    db.HemoCapa.Add(c);E("AssignCapa","RcaOpen","RcaOpen",officer,13);E("PublishRca","RcaOpen","CapaMonitoring",officer,14);
                }
            }
            await db.SaveChangesAsync(ct);
        }
    }
}
