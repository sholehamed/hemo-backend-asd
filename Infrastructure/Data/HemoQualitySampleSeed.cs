using Application.Core.Quality_core;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Infrastructure.Data;
// Explicit educational data, independent of ST05/06. No operational certificates or clinical approval.
internal static class HemoQualitySampleSeed
{
    private const string Prefix="HEMO-DEMO-ST07:";
    private const string Note="اطلاعات ساختگی آموزشی ST-07 — فقط تمرین نرم‌افزار؛ نه تأیید فنی یا بالینی";
    private static T Mark<T>(T e,string key,Guid actor,DateTime now) where T:AuditedEntity
    {e.ConvertID=Prefix+key;e.Created=now;e.CreatedBy=actor;e.CreatedByIP="demo";e.LastModified=now;e.LastModifiedBy=actor;e.LastModifiedByIP="demo";return e;}
    public static async Task ApplyAsync(ApplicationDbContext db,Guid org,CancellationToken ct)
    {
        if(!await db.OrgLocation.AnyAsync(x=>x.GUID==org&&x.ConvertID=="HEMO-DEMO-ST02:Facility",ct))throw new InvalidOperationException("ST07 requires the dedicated demo facility.");
        async Task<UserRole> Actor(string u)=>await db.UserRole.Include(x=>x.User).SingleAsync(x=>x.OrgLocationGUID==org&&x.User.Username==u,ct);
        var tech=await Actor("hemo.equipment.tech");var reviewer=await Actor("hemo.equipment.reviewer");var officer=await Actor("hemo.quality.officer");var qr=await Actor("hemo.quality.reviewer");var bank=await Actor("hemo.bloodbank");var nurse=await Actor("hemo.nurse");
        var now=DateTime.UtcNow;var past=now.AddHours(-2);
        var equipment=new[]{("DEMO-ST07-FRIDGE","یخچال آموزشی","Refrigerator","Operational"),("DEMO-ST07-CENTRIFUGE","سانتریفیوژ آموزشی","Centrifuge","Hold"),("DEMO-ST07-FREEZER","فریزر آموزشی سررسید گذشته","Freezer","Hold"),("DEMO-ST07-WATERBATH","بن‌ماری آموزشی","WaterBath","Hold")};
        foreach(var f in equipment)
        {
            var existing=await db.HemoEquipment.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.Code==f.Item1,ct);
            if(existing!=null){if(existing.ConvertID!=Prefix+f.Item1)throw new InvalidOperationException("Conflicting ST07 equipment.");continue;}
            var e=Mark(new HemoEquipment{OrgLocationGUID=org,Code=f.Item1,Name=f.Item2,Category=f.Item3,BrandModel="DEMO-ONLY",Ward="بانک خون آموزشی",SerialNumber=f.Item1,Status=f.Item4,CalibrationAt=past.AddDays(-1),CalibrationDueAt=f.Item1.EndsWith("FREEZER")?past.AddDays(-1):now.AddDays(30)},f.Item1,tech.UserGUID,past);
            db.HemoEquipment.Add(e);
            void E(string kind,string before,string after,UserRole a,DateTime time,object? data=null){db.HemoEquipmentEvent.Add(Mark(new HemoEquipmentEvent{HemoEquipmentGUID=e.GUID,Kind=kind,PreviousStatus=before,NewStatus=after,OccurredAt=time,ActorUserGUID=a.UserGUID,ActorRoleGUID=a.RoleGUID,Notes=Note,Reference="DEMO-ONLY",EvidenceJson=data==null?null:JsonSerializer.Serialize(data)},f.Item1+":"+kind,a.UserGUID,time));}
            E("Register","","Hold",tech,past.AddMinutes(-3));E("Calibration","Hold","Hold",tech,past.AddMinutes(-2),new{Result="Accepted",Note});
            if(e.Status=="Operational")E("Release","Hold","Operational",reviewer,past.AddMinutes(-1));
            if(f.Item1.EndsWith("CENTRIFUGE")){var incident=Mark(new HemoEquipmentIncident{HemoEquipmentGUID=e.GUID,IncidentNumber="DEMO-EQI-01",Urgency="Routine",Status="Open",ReportedAt=past,ReporterUserGUID=tech.UserGUID,Description=Note},f.Item1+":incident",tech.UserGUID,past);db.HemoEquipmentIncident.Add(incident);E("IncidentReport","Hold","Hold",tech,past,new{IncidentGuid=incident.GUID});}
            if(f.Item1.EndsWith("FRIDGE")){db.BankTemperatureLog.Add(Mark(new BankTemperatureLog{OrgLocationGUID=org,EquipmentCode=e.Code,MeasuredAt=past,RecordedAt=past,Temperature=0m,Assessment="Acceptable",Notes=Note,ActorUserGUID=tech.UserGUID,ActorRoleGUID=tech.RoleGUID},f.Item1+":temp",tech.UserGUID,past));}
            await db.SaveChangesAsync(ct);
        }
        // Register existing demo-only ST05/ST06 codes as held until explicit technical review, without editing historical logs or units.
        // Do not seed them automatically: preserve the ability to exercise prior stages.
        async Task Create(string kind,string key,string title,UserRole actor,QualityHeader header,QualityLineData data,Guid? owner=null,DateTime? due=null,string status="Draft")
        {
            var number="DEMO-"+key;var existing=await db.HemoQualityRecord.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.OrgLocationGUID==org&&x.Number==number,ct);
            if(existing!=null){if(existing.ConvertID!=Prefix+key)throw new InvalidOperationException("Conflicting ST07 quality record.");return;}
            var r=Mark(new HemoQualityRecord{OrgLocationGUID=org,Kind=kind,Number=number,Title=title,Ward="بانک خون آموزشی",Status=status,OccurredAt=past,CreatedByUserGUID=actor.UserGUID,DetailsJson=JsonSerializer.Serialize(header)},key,actor.UserGUID,past);db.HemoQualityRecord.Add(r);
            var l=Mark(new HemoQualityLine{HemoQualityRecordGUID=r.GUID,Title=kind=="Committee"?"تمرین پیگیری مصوبه":kind=="Qc"?"معرف ساختگی":kind=="Audit"?"بند آموزشی بررسی مستندات":"کاربر آزمایشی پرستار",DetailsJson=JsonSerializer.Serialize(data),OwnerUserGUID=owner,DueAt=due,Status=status=="Approved"?"Open":"Draft"},key+":line",actor.UserGUID,past);db.HemoQualityLine.Add(l);
            void E(string act,string before,string next,UserRole a){db.HemoQualityEvent.Add(Mark(new HemoQualityEvent{HemoQualityRecordGUID=r.GUID,ActorUserGUID=a.UserGUID,ActorRoleGUID=a.RoleGUID,Action=act,PreviousStatus=before,NewStatus=next,OccurredAt=past,Notes=Note},key+":"+act,a.UserGUID,past));}
            E("Create","","Draft",actor);E("SaveLine","Draft","Draft",actor);
            if(status!="Draft")E("Submit","Draft","Submitted",actor);
            if(status=="Approved"){r.ReviewedAt=past;r.ReviewedBy=qr.UserGUID;r.ReviewNotes=Note;E("Approve","Submitted","Approved",qr);}
            await db.SaveChangesAsync(ct);
        }
        await Create("Qc","ST07-QC-01","کنترل کیفیت آزمایشی آماده بازبینی",bank,new QualityHeader(Shift:"Morning",Operator:"کارشناس آموزشی"),new QualityLineData(Lot:"DEMO-LOT",Manufacturer:"DEMO-ONLY",Expiry:new DateTimeOffset(now.AddDays(7)),PositiveExpected:"ثبت ساختگی",NegativeExpected:"ثبت ساختگی",PositiveObserved:"ثبت ساختگی",NegativeObserved:"ثبت ساختگی",Result:"Accepted",Notes:Note),status:"Submitted");
        await Create("Committee","ST07-BTC-01","جلسه آموزشی و مصوبه باز",officer,new QualityHeader(Chair:"رئیس آموزشی",Secretary:"دبیر آموزشی",Participants:Note,Agenda:Note),new QualityLineData(Indicator:"مستند تمرین",Notes:Note),nurse.UserGUID,now.AddDays(7),"Approved");
        await Create("Audit","ST07-AUD-01","ممیزی آموزشی در پیش‌نویس",officer,new QualityHeader(Checklist:"چک‌لیست آموزشی غیررسمی",Auditor:"ممیز آموزشی"),new QualityLineData(Clause:"DEMO-01",Score:1,Notes:Note));
        await Create("Training","ST07-TRN-01","دوره آموزشی آماده ثبت حضور",officer,new QualityHeader(Instructor:"مدرس آموزشی",DurationMinutes:60,Capacity:10,Audience:"کارکنان آموزشی",MaterialReference:"DEMO-ONLY"),new QualityLineData(PersonnelCode:"DEMO-STAFF",Ward:"بانک خون آموزشی",Shift:"Morning",Attendance:"Registered"),nurse.UserGUID);
        if(!await db.HemoFacilityProfile.IgnoreQueryFilters().AnyAsync(x=>x.OrgLocationGUID==org,ct))db.HemoFacilityProfile.Add(Mark(new HemoFacilityProfile{OrgLocationGUID=org,FacilityName="بیمارستان آزمایشی HEMO",AccreditationCode="DEMO-ONLY",ReportFooter=Note},"facility",officer.UserGUID,past));
        await db.SaveChangesAsync(ct);
    }
}
