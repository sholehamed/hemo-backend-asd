using Application.Repository.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Data;

// Explicitly opt-in development seed. No shared default passwords or changes to existing accounts.
public static class HemoDemoSeed
{
    public sealed record Account(string Key,string Username,string RoleName,string FamilyName,string[] Permissions);
    private static readonly string[] ReadKeys=["stats","list","detail","find","history","reviews"];
    public static readonly Account[] Accounts=[
        new("Admission","hemo.admission","HEMO | کارشناس پذیرش","پذیرش",[..ReadKeys,"create","update","review"]),
        new("Supervisor","hemo.supervisor","HEMO | مسئول پذیرش","مسئول پذیرش",[..ReadKeys,"create","update","review","identity"]),
        new("Viewer","hemo.viewer","HEMO | مشاهده پذیرش","مشاهده",ReadKeys)
    ];
    public static readonly Account[] BloodRequestAccounts=[
        new("Nurse","hemo.nurse","HEMO | پرستار","پرستار",["list","detail","request-list","request-detail","request-stats","request-nurse","request-edit","request-submit","request-cancel"]),
        new("Doctor","hemo.doctor","HEMO | پزشک","پزشک",["list","detail","request-list","request-detail","request-stats","request-doctor","request-edit","request-submit","request-cancel","request-decide"]),
        new("BloodBank","hemo.bloodbank","HEMO | کارشناس بانک خون","بانک خون",["request-bank","request-bank-detail"])
    ];
    public static readonly string[] BankReadKeys=["bank-inventory","bank-unit","bank-stats","bank-temperature-history","bank-lab-history"];
    public static readonly string[] BankOperatorKeys=[..BankReadKeys,"bank-receive","bank-test","bank-reserve","bank-thaw","bank-ready","bank-hold","bank-unreserve","bank-temperature"];
    public static readonly string[] BankSupervisorKeys=[..BankOperatorKeys,"bank-inspect","bank-discard","bank-return"];
    public static readonly Account BankSupervisor=new("BankSupervisor","hemo.bank.supervisor","HEMO | مسئول بانک خون","مسئول بانک خون",["request-bank","request-bank-detail",..BankSupervisorKeys]);
    public static readonly (string Key,string Method,string Path)[] BankEndpoints=[
        ("bank-inventory","post","/Identity/BloodBank/Inventory"),("bank-unit","get","/Identity/BloodBank/Unit/{guid}"),("bank-stats","post","/Identity/BloodBank/Stats"),("bank-temperature-history","post","/Identity/BloodBank/TemperatureHistory"),
        ("bank-lab-history","post","/Identity/BloodBank/LabHistory"),("bank-receive","post","/Identity/BloodBank/Receive"),("bank-inspect","post","/Identity/BloodBank/Inspect"),("bank-test","post","/Identity/BloodBank/Test"),("bank-reserve","post","/Identity/BloodBank/Reserve"),
        ("bank-thaw","post","/Identity/BloodBank/Thaw"),("bank-ready","post","/Identity/BloodBank/Ready"),("bank-hold","post","/Identity/BloodBank/Hold"),("bank-unreserve","post","/Identity/BloodBank/Unreserve"),
        ("bank-discard","post","/Identity/BloodBank/Discard"),("bank-temperature","post","/Identity/BloodBank/Temperature"),("bank-return","post","/Identity/BloodBank/ReturnRequest")
    ];
    public static readonly string[] TransferReadKeys=["tx-list","tx-detail","tx-stats"];
    public static readonly string[] TransferNurseKeys=[..TransferReadKeys,"tx-receive","tx-receipt-hold","tx-verify","tx-witness","tx-start","tx-vital","tx-complete","tx-stop","tx-return"];
    public static readonly string[] TransferSupervisorKeys=[..TransferReadKeys,"tx-candidates","tx-dispatch","tx-return","tx-accept-return","tx-dispose"];
    public static readonly Account WitnessNurse=new("WitnessNurse","hemo.nurse.witness","HEMO | پرستار دوم / شاهد","پرستار شاهد",[..TransferReadKeys,"tx-witness"]);
    public static readonly (string Key,string Method,string Path)[] TransferEndpoints=[
        ("tx-list","post","/Identity/Transfusion/List"),("tx-detail","get","/Identity/Transfusion/Detail/{guid}"),("tx-stats","post","/Identity/Transfusion/Stats"),
        ("tx-candidates","post","/Identity/Transfusion/Candidates"),("tx-dispatch","post","/Identity/Transfusion/Dispatch"),
        ("tx-receive","post","/Identity/Transfusion/Receive"),("tx-receipt-hold","post","/Identity/Transfusion/ReceiptHold"),
        ("tx-verify","post","/Identity/Transfusion/Verify"),("tx-witness","post","/Identity/Transfusion/Witness"),("tx-start","post","/Identity/Transfusion/Start"),
        ("tx-vital","post","/Identity/Transfusion/Vital"),("tx-complete","post","/Identity/Transfusion/Complete"),("tx-stop","post","/Identity/Transfusion/Stop"),
        ("tx-return","post","/Identity/Transfusion/Return"),("tx-accept-return","post","/Identity/Transfusion/AcceptReturn"),("tx-dispose","post","/Identity/Transfusion/Dispose"),("tx-doctor-note","post","/Identity/Transfusion/DoctorNote")
    ];
    public static readonly string[] HvReadKeys=["hv-list","hv-detail","hv-stats"];
    public static readonly Account HvOfficer=new("HvOfficer","hemo.hv.officer","HEMO | مسئول هموویژیلانس","هموویژیلانس",[..HvReadKeys,"hv-candidates","hv-report","hv-observe","hv-investigate","hv-return","hv-rca","hv-assign","hv-owners","hv-external","hv-reports","hv-report-detail","hv-archive","hv-report-external"]);
    public static readonly Account HvReviewer=new("HvReviewer","hemo.hv.reviewer","HEMO | بازبین مستقل هموویژیلانس","بازبین مستقل",[..HvReadKeys,"hv-verify","hv-close","hv-reports","hv-report-detail"]);
    public static readonly (string Key,string Method,string Path)[] HvEndpoints=[
        ("hv-list","post","/Identity/Hemovigilance/List"),
        ("hv-detail","get","/Identity/Hemovigilance/Detail/{guid}"),
        ("hv-stats","post","/Identity/Hemovigilance/Stats"),
        ("hv-candidates","post","/Identity/Hemovigilance/Candidates"),
        ("hv-report","post","/Identity/Hemovigilance/Report"),
        ("hv-observe","post","/Identity/Hemovigilance/Observe"),
        ("hv-doctor","post","/Identity/Hemovigilance/DoctorReview"),
        ("hv-bank","post","/Identity/Hemovigilance/BankReview"),
        ("hv-investigate","post","/Identity/Hemovigilance/Investigate"),
        ("hv-return","post","/Identity/Hemovigilance/ReturnToDoctor"),
        ("hv-rca","post","/Identity/Hemovigilance/PublishRca"),
        ("hv-assign","post","/Identity/Hemovigilance/AssignCapa"),
        ("hv-owners","get","/Identity/Hemovigilance/Owners"),
        ("hv-progress","post","/Identity/Hemovigilance/CapaProgress"),
        ("hv-verify","post","/Identity/Hemovigilance/CapaVerify"),
        ("hv-close","post","/Identity/Hemovigilance/Close"),
        ("hv-external","post","/Identity/Hemovigilance/External"),
        ("hv-reports","post","/Identity/Hemovigilance/Reports"),
        ("hv-report-detail","get","/Identity/Hemovigilance/ReportDetail/{guid}"),
        ("hv-archive","post","/Identity/Hemovigilance/Archive"),
        ("hv-report-external","post","/Identity/Hemovigilance/ReportExternal")
    ];
    public static readonly (string Key,string Method,string Path)[] QualityEndpoints=[
        ("quality-equipment-list","post","/Identity/HemoQuality/EquipmentList"),
        ("quality-equipment-detail","get","/Identity/HemoQuality/EquipmentDetail/{guid}"),
        ("quality-equipment-stats","post","/Identity/HemoQuality/EquipmentStats"),
        ("quality-equipment-create","post","/Identity/HemoQuality/EquipmentCreate"),
        ("quality-equipment-edit","post","/Identity/HemoQuality/EquipmentEdit"),
        ("quality-equipment-service","post","/Identity/HemoQuality/EquipmentService"),
        ("quality-equipment-hold","post","/Identity/HemoQuality/EquipmentHold"),
        ("quality-equipment-release","post","/Identity/HemoQuality/EquipmentRelease"),
        ("quality-equipment-archive","post","/Identity/HemoQuality/EquipmentArchive"),
        ("quality-equipment-temperature","post","/Identity/HemoQuality/EquipmentTemperature"),
        ("quality-incidents","post","/Identity/HemoQuality/Incidents"),
        ("quality-incident-report","post","/Identity/HemoQuality/IncidentReport"),
        ("quality-incident-resolve","post","/Identity/HemoQuality/IncidentResolve"),
        ("quality-facility","get","/Identity/HemoQuality/Facility"),
        ("quality-facility-save","post","/Identity/HemoQuality/FacilitySave"),
        ("quality-stats","post","/Identity/HemoQuality/Stats"),
        ("quality-qc-list","post","/Identity/HemoQuality/QcList"),
        ("quality-qc-detail","get","/Identity/HemoQuality/QcDetail/{guid}"),
        ("quality-qc-create","post","/Identity/HemoQuality/QcCreate"),
        ("quality-qc-edit","post","/Identity/HemoQuality/QcEdit"),
        ("quality-qc-line","post","/Identity/HemoQuality/QcLine"),
        ("quality-qc-submit","post","/Identity/HemoQuality/QcSubmit"),
        ("quality-qc-approve","post","/Identity/HemoQuality/QcApprove"),
        ("quality-committee-list","post","/Identity/HemoQuality/CommitteeList"),
        ("quality-committee-detail","get","/Identity/HemoQuality/CommitteeDetail/{guid}"),
        ("quality-committee-create","post","/Identity/HemoQuality/CommitteeCreate"),
        ("quality-committee-edit","post","/Identity/HemoQuality/CommitteeEdit"),
        ("quality-committee-line","post","/Identity/HemoQuality/CommitteeLine"),
        ("quality-committee-submit","post","/Identity/HemoQuality/CommitteeSubmit"),
        ("quality-committee-approve","post","/Identity/HemoQuality/CommitteeApprove"),
        ("quality-committee-progress","post","/Identity/HemoQuality/CommitteeProgress"),
        ("quality-committee-verify","post","/Identity/HemoQuality/CommitteeVerify"),
        ("quality-committee-close","post","/Identity/HemoQuality/CommitteeClose"),
        ("quality-committee-owners","get","/Identity/HemoQuality/CommitteeOwners"),
        ("quality-audit-list","post","/Identity/HemoQuality/AuditList"),
        ("quality-audit-detail","get","/Identity/HemoQuality/AuditDetail/{guid}"),
        ("quality-audit-create","post","/Identity/HemoQuality/AuditCreate"),
        ("quality-audit-edit","post","/Identity/HemoQuality/AuditEdit"),
        ("quality-audit-line","post","/Identity/HemoQuality/AuditLine"),
        ("quality-audit-submit","post","/Identity/HemoQuality/AuditSubmit"),
        ("quality-audit-approve","post","/Identity/HemoQuality/AuditApprove"),
        ("quality-training-list","post","/Identity/HemoQuality/TrainingList"),
        ("quality-training-detail","get","/Identity/HemoQuality/TrainingDetail/{guid}"),
        ("quality-training-create","post","/Identity/HemoQuality/TrainingCreate"),
        ("quality-training-edit","post","/Identity/HemoQuality/TrainingEdit"),
        ("quality-training-line","post","/Identity/HemoQuality/TrainingLine"),
        ("quality-training-submit","post","/Identity/HemoQuality/TrainingSubmit"),
        ("quality-training-approve","post","/Identity/HemoQuality/TrainingApprove"),
        ("quality-training-owners","get","/Identity/HemoQuality/TrainingOwners")
    ];
    public static readonly Account[] QualityAccounts=[
        new("EquipmentTech","hemo.equipment.tech","HEMO | کارشناس تجهیزات","کارشناس تجهیزات",["quality-equipment-list","quality-equipment-detail","quality-equipment-stats","quality-equipment-create","quality-equipment-edit","quality-equipment-service","quality-equipment-hold","quality-equipment-temperature","quality-incidents","quality-incident-report","quality-incident-resolve"]),
        new("EquipmentReviewer","hemo.equipment.reviewer","HEMO | بازبین فنی تجهیزات","بازبین فنی تجهیزات",["quality-equipment-list","quality-equipment-detail","quality-equipment-stats","quality-equipment-release","quality-equipment-archive","quality-incidents"]),
        new("QualityOfficer","hemo.quality.officer","HEMO | مسئول کیفیت","مسئول کیفیت",["quality-qc-list","quality-qc-detail","quality-qc-create","quality-qc-edit","quality-qc-line","quality-qc-submit","quality-committee-list","quality-committee-detail","quality-committee-create","quality-committee-edit","quality-committee-line","quality-committee-submit","quality-committee-owners","quality-audit-list","quality-audit-detail","quality-audit-create","quality-audit-edit","quality-audit-line","quality-audit-submit","quality-training-list","quality-training-detail","quality-training-create","quality-training-edit","quality-training-line","quality-training-submit","quality-training-owners","quality-facility","quality-facility-save","quality-stats"]),
        new("QualityReviewer","hemo.quality.reviewer","HEMO | بازبین مستقل کیفیت","بازبین مستقل کیفیت",["quality-qc-list","quality-qc-detail","quality-qc-approve","quality-committee-list","quality-committee-detail","quality-committee-approve","quality-committee-verify","quality-committee-close","quality-audit-list","quality-audit-detail","quality-audit-approve","quality-training-list","quality-training-detail","quality-training-approve","quality-facility","quality-stats"])
    ];
    public static Account[] DemoAccounts(bool includeRequests,bool includeBank,bool includeTransfusion=false,bool includeHv=false,bool includeQuality=false)
    {
        if(includeQuality)return DemoAccounts(true,true,true,true).Select(a=>a.Key switch
        {
            "Nurse"=>a with{Permissions=[..a.Permissions,"quality-committee-list","quality-committee-detail","quality-committee-progress"]},
            "Doctor"=>a with{Permissions=[..a.Permissions,"quality-committee-list","quality-committee-detail","quality-committee-progress"]},
            "BloodBank"=>a with{Permissions=[..a.Permissions,"quality-equipment-list","quality-equipment-detail","quality-equipment-temperature","quality-incidents","quality-incident-report","quality-qc-list","quality-qc-detail","quality-qc-create","quality-qc-edit","quality-qc-line","quality-qc-submit","quality-committee-list","quality-committee-detail","quality-committee-progress"]},
            "BankSupervisor"=>a with{Permissions=[..a.Permissions,"quality-equipment-list","quality-equipment-detail","quality-equipment-stats","quality-incidents","quality-incident-report","quality-qc-list","quality-qc-detail","quality-qc-approve","quality-committee-list","quality-committee-detail","quality-committee-progress","quality-stats"]},
            "HvOfficer"=>a with{Permissions=[..a.Permissions,"quality-committee-list","quality-committee-detail","quality-audit-list","quality-audit-detail","quality-training-list","quality-training-detail","quality-facility","quality-stats"]},
            "HvReviewer"=>a with{Permissions=[..a.Permissions,"quality-committee-list","quality-committee-detail","quality-audit-list","quality-audit-detail","quality-training-list","quality-training-detail","quality-stats"]},
            _=>a
        }).Concat(QualityAccounts).ToArray();
        if(includeHv)return DemoAccounts(true,true,true).Select(a=>a.Key switch
        {
            "Nurse"=>a with{Permissions=[..a.Permissions,..HvReadKeys,"hv-candidates","hv-report","hv-observe","hv-progress"]},
            "Doctor"=>a with{Permissions=[..a.Permissions,..HvReadKeys,"hv-doctor","hv-observe","hv-progress"]},
            "BloodBank" or "BankSupervisor"=>a with{Permissions=[..a.Permissions,..HvReadKeys,"hv-bank","hv-observe","hv-progress"]},
            _=>a
        }).Append(HvOfficer).Append(HvReviewer).ToArray();
        if(includeTransfusion)return DemoAccounts(true,true).Select(a=>a.Key switch
        {
            "Nurse"=>a with{Permissions=[..a.Permissions,..TransferNurseKeys]},
            "Doctor"=>a with{Permissions=[..a.Permissions,..TransferReadKeys,"tx-doctor-note","tx-stop"]},
            "BloodBank"=>a with{Permissions=[..a.Permissions,..TransferReadKeys,"tx-accept-return"]},
            "BankSupervisor"=>a with{Permissions=[..a.Permissions,..TransferSupervisorKeys]},
            _=>a
        }).Append(WitnessNurse).ToArray();
        if(includeBank)return Accounts.Concat(BloodRequestAccounts.Select(a=>a.Key=="BloodBank"?a with{Permissions=[..a.Permissions,..BankOperatorKeys]}:a)).Append(BankSupervisor).ToArray();
        return includeRequests?Accounts.Concat(BloodRequestAccounts).ToArray():Accounts;
    }
    public static readonly (string Key,string Method,string Path)[] BloodRequestEndpoints=[
        ("request-list","post","/Identity/BloodRequest/BindPaging"),
        ("request-detail","get","/Identity/BloodRequest/{guid}"),
        ("request-stats","post","/Identity/BloodRequest/Stats"),
        ("request-nurse","post","/Identity/BloodRequest/Nurse"),
        ("request-doctor","post","/Identity/BloodRequest/Doctor"),
        ("request-edit","put","/Identity/BloodRequest"),
        ("request-submit","post","/Identity/BloodRequest/Submit"),
        ("request-cancel","post","/Identity/BloodRequest/Cancel"),
        ("request-decide","post","/Identity/BloodRequest/Decide"),
        ("request-bank","post","/Identity/BloodRequest/BankQueue"),
        ("request-bank-detail","get","/Identity/BloodRequest/Bank/{guid}")
    ];
    public static readonly (string Key,string Method,string Path)[] Endpoints=[
        ("stats","post","/Identity/PatientAdmission/Stats"),("list","post","/Identity/PatientAdmission/BindPaging"),
        ("detail","get","/Identity/PatientAdmission/{guid}"),("find","get","/Identity/PatientAdmission/FindPatient"),
        ("history","get","/Identity/PatientAdmission/{guid}/History"),("reviews","get","/Identity/PatientAdmission/{guid}/Reviews"),
        ("create","post","/Identity/PatientAdmission"),("update","put","/Identity/PatientAdmission"),
        ("review","post","/Identity/PatientAdmission/Review"),("identity","put","/Identity/PatientAdmission/PatientIdentity")
    ];
    // Opt-in aggregate-only managerial role; no patient detail, list or clinical write grants.
    public static readonly Account DashboardManager=new("DashboardManager","hemo.dashboard.manager","HEMO | مدیر آمار بیمارستان","مدیر آمار",["stats","request-stats","bank-stats","tx-stats","hv-stats","quality-equipment-stats","quality-stats"]);
    private const string Prefix="HEMO-DEMO-ST02:";
    public static async Task SeedHemoDemoAsync(this WebApplication app)
    {
        if(!app.Configuration.GetValue<bool>("HemoSeed:Enabled"))return;
        if(!app.Environment.IsDevelopment())throw new InvalidOperationException("HEMO demo seed is restricted to Development.");
        using var scope=app.Services.CreateScope();
        await ApplyAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),app.Configuration,CancellationToken.None);
    }
    public static async Task ApplyAsync(ApplicationDbContext db,IConfiguration config,CancellationToken ct)
    {
        var includeDashboardSamples=config.GetValue<bool>("HemoSeed:IncludeDashboardSamples");
        var includeDeliverySamples=includeDashboardSamples||config.GetValue<bool>("HemoSeed:IncludeDeliverySamples");
        var includeQualitySamples=includeDeliverySamples||config.GetValue<bool>("HemoSeed:IncludeQualitySamples");
        var includeQuality=includeQualitySamples||config.GetValue<bool>("HemoSeed:IncludeQuality");
        var includeHvSamples=includeDeliverySamples||config.GetValue<bool>("HemoSeed:IncludeHemovigilanceSamples");
        var includeHv=includeQuality||includeHvSamples||config.GetValue<bool>("HemoSeed:IncludeHemovigilance");
        var includeSamples=includeDeliverySamples||config.GetValue<bool>("HemoSeed:IncludeTransfusionSamples");
        var includeTransfusion=includeHv||includeSamples||config.GetValue<bool>("HemoSeed:IncludeTransfusion");
        var includeBank=includeTransfusion||config.GetValue<bool>("HemoSeed:IncludeBloodBank");
        var includeRequests=includeBank||config.GetValue<bool>("HemoSeed:IncludeBloodRequests");
        var accounts=DemoAccounts(includeRequests,includeBank,includeTransfusion,includeHv,includeQuality);
        if(includeDashboardSamples)accounts=[..accounts,DashboardManager];
        var pending=new Dictionary<string,string>();
        foreach(var account in accounts)
        {
            var existing=await db.User.IgnoreQueryFilters().SingleOrDefaultAsync(u=>u.Username==account.Username,ct);
            if(existing!=null && (existing.IsDelete || existing.ConvertID!=Prefix+account.Key))
                throw new InvalidOperationException("Existing username conflicts with HEMO seed: "+account.Username);
            if(existing==null)
            {
                var password=config["HemoSeed:Passwords:"+account.Key];
                if(string.IsNullOrWhiteSpace(password)||password.Length<16||password.Length>256 ||
                   await db.PasswordBlockList.AnyAsync(p=>p.Password==password,ct))
                    throw new InvalidOperationException("Supply a unique password of 16+ characters for "+account.Key);
                pending.Add(account.Key,password);
            }
        }
        if(pending.Values.Distinct(StringComparer.Ordinal).Count()!=pending.Count)
            throw new InvalidOperationException("HEMO seed passwords must be distinct.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var root=await db.Role.SingleOrDefaultAsync(r=>r.Gkey==1,ct);
        if(root==null)
        {
            if(await db.Role.IgnoreQueryFilters().AnyAsync(ct))throw new InvalidOperationException("Existing identity root role is missing.");
            root=new Role{Name="نقش ریشه (ساختاری)",Path="/1/",Gkey=1,Level=1};
            db.Role.Add(root);await db.SaveChangesAsync(ct);
        }
        // Gkey 2 is reserved by the existing general-login flow. Never assign it to a HEMO clinical role.
        if(!await db.Role.IgnoreQueryFilters().AnyAsync(r=>r.Gkey>=2,ct))
        {
            db.Role.Add(new Role{Name="نقش سیستمی رزرو شده",PRoleGUID=root.GUID});await db.SaveChangesAsync(ct);
        }
        var org=await db.OrgLocation.SingleOrDefaultAsync(o=>o.ConvertID==Prefix+"Facility",ct);
        if(org==null)
        {
            org=new OrgLocation{Name="بیمارستان آزمایشی HEMO",ConvertID=Prefix+"Facility"};
            db.OrgLocation.Add(org);await db.SaveChangesAsync(ct);
        }
        // Reuse the identity subsystem; only create its metadata on an otherwise empty demo database.
        var identityGuid=Guid.Parse("41e8f63b-ac36-44bb-9bae-2d3355b1b743");
        var subsystem=await db.SubSystem.SingleOrDefaultAsync(s=>s.GUID==identityGuid,ct);
        if(subsystem==null)
        {
            if(await db.SubSystem.AnyAsync(ct))throw new InvalidOperationException("Identity subsystem must already be registered.");
            var url=new Uri(config["HemoSeed:ApiBaseUrl"]??"http://127.0.0.1:1002");
            subsystem=new SubSystem{GUID=identityGuid,Name="HEMO Identity",Host=url.Host,Port=url.Port,Active=true};
            db.SubSystem.Add(subsystem);await db.SaveChangesAsync(ct);
        }
        if(!subsystem.Active)throw new InvalidOperationException("Identity subsystem is inactive.");
        var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/admission",ct);
        if(menu==null)
        {
            menu=new Menu{label="پذیرش بیمارستان",routerLink="/admission",icon="pi pi-id-card",ShowInMenu=true,Gorder=1,IsPublic=false};
            db.Menu.Add(menu);await db.SaveChangesAsync(ct);
        }
        Menu? requestMenu=null;
        if(includeRequests)
        {
            requestMenu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/hemo/requests",ct);
            if(requestMenu==null)
            {
                requestMenu=new Menu{label="درخواست خون",routerLink="/hemo/requests",icon="pi pi-file-edit",ShowInMenu=true,Gorder=2,IsPublic=false};
                db.Menu.Add(requestMenu);await db.SaveChangesAsync(ct);
            }
        }
        Menu? bankMenu=null;
        if(includeBank)
        {
            bankMenu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/hemo/bank",ct);
            if(bankMenu==null){bankMenu=new Menu{label="مدیریت بانک خون",routerLink="/hemo/bank",icon="pi pi-inbox",ShowInMenu=true,Gorder=3,IsPublic=false};db.Menu.Add(bankMenu);await db.SaveChangesAsync(ct);}
        }
        Menu? transferMenu=null;
        if(includeTransfusion)
        {
            transferMenu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/hemo/transfusion",ct);
            if(transferMenu==null){transferMenu=new Menu{label="تحویل و پایش تزریق",routerLink="/hemo/transfusion",icon="pi pi-heart",ShowInMenu=true,Gorder=4,IsPublic=false};db.Menu.Add(transferMenu);await db.SaveChangesAsync(ct);}
        }
        Menu? hvMenu=null;
        if(includeHv)
        {
            hvMenu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/hemo/hv",ct);
            if(hvMenu==null){hvMenu=new Menu{label="واکنش و هموویژیلانس",routerLink="/hemo/hv",icon="pi pi-shield",ShowInMenu=true,Gorder=5,IsPublic=false};db.Menu.Add(hvMenu);await db.SaveChangesAsync(ct);}
        }
        Menu? qualityMenu=null;
        if(includeQuality)
        {
            qualityMenu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/hemo/quality",ct);
            if(qualityMenu==null){qualityMenu=new Menu{label="تجهیزات و مدیریت کیفیت",routerLink="/hemo/quality",icon="pi pi-cog",ShowInMenu=true,Gorder=6,IsPublic=false};db.Menu.Add(qualityMenu);await db.SaveChangesAsync(ct);}
        }
        var apis=new Dictionary<string,Guid>();
        var endpointMenus=new Dictionary<string,Guid>();
        foreach(var e in (includeRequests?Endpoints.Concat(BloodRequestEndpoints):Endpoints).Concat(includeBank?BankEndpoints:Array.Empty<(string,string,string)>()).Concat(includeTransfusion?TransferEndpoints:Array.Empty<(string,string,string)>()).Concat(includeHv?HvEndpoints:Array.Empty<(string,string,string)>()).Concat(includeQuality?QualityEndpoints:Array.Empty<(string,string,string)>()).Concat(new[]{("login","post","/Identity/User/Login"),("hemo-login","post","/Identity/HemoSession/Login")}))
        {
            var api=await db.Api.FirstOrDefaultAsync(a=>a.SubSystemGUID==subsystem.GUID && a.APITypeName==e.Item2 && a.DownstreamPathTemplate==e.Item3,ct);
            if(api==null)
            {
                api=new Api{SubSystemGUID=subsystem.GUID,APITypeName=e.Item2,DownstreamPathTemplate=e.Item3,UpstreamPathTemplate=e.Item3,Title="HEMO "+e.Item1,Active=true,IsPublic=false};
                db.Api.Add(api);await db.SaveChangesAsync(ct);
            }
            if(!api.Active)throw new InvalidOperationException("HEMO API is inactive: "+e.Item3);
            apis[e.Item1]=api.GUID;
            if(e.Item1 is "login" or "hemo-login")continue;
            var targetMenu=e.Item1.StartsWith("quality-")?qualityMenu!:e.Item1.StartsWith("hv-")?hvMenu!:e.Item1.StartsWith("tx-")?transferMenu!:e.Item1.StartsWith("bank-")?bankMenu!:e.Item1.StartsWith("request-")?requestMenu!:menu;
            endpointMenus[e.Item1]=targetMenu.GUID;
            if(!await db.MenuApi.AnyAsync(m=>m.MenuGUID==targetMenu.GUID&&m.ApiGUID==api.GUID,ct))
            {db.MenuApi.Add(new MenuApi{MenuGUID=targetMenu.GUID,ApiGUID=api.GUID});await db.SaveChangesAsync(ct);}
        }
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleOrDefaultAsync(r=>r.ConvertID==Prefix+account.Key,ct);
            if(role==null)
            {
                role=new Role{Name=account.RoleName,PRoleGUID=root.GUID,OrgLocationGUID=org.GUID,ConvertID=Prefix+account.Key};
                db.Role.Add(role);await db.SaveChangesAsync(ct);
            }
            foreach(var key in account.Permissions)
            {
                var id=apis[key];var menuId=endpointMenus[key];
                if(!await db.RoleAccess.AnyAsync(r=>r.RoleGUID==role.GUID&&r.MenuGUID==menuId&&r.ApiGUID==id,ct))
                {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.GUID,MenuGUID=menuId,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
            var user=await db.User.SingleOrDefaultAsync(u=>u.Username==account.Username,ct);
            if(user==null)
            {
                user=new User{Username=account.Username,FirstName="کاربر آزمایشی",LastName=account.FamilyName,ConvertID=Prefix+account.Key,MobileIsConfirm=true,ForceChangePassword=false};
                user.Password=UserService.HashPassword(user.GUID,pending[account.Key]);
                db.User.Add(user);await db.SaveChangesAsync(ct);
            }
            if(!await db.UserRole.AnyAsync(r=>r.UserGUID==user.GUID&&r.RoleGUID==role.GUID&&r.OrgLocationGUID==org.GUID,ct))
            {
                db.UserRole.Add(new UserRole{UserGUID=user.GUID,RoleGUID=role.GUID,OrgLocationGUID=org.GUID,ExpireDate=DateTime.Now.AddYears(1)});
                await db.SaveChangesAsync(ct);
            }
        }
        var includeCompanyMenuPilot=config.GetValue<bool>("HemoSeed:IncludeCompanyMenuPilot");
        var includeAdmissionReadPages=config.GetValue<bool>("HemoSeed:IncludeAdmissionReadPages");
        var includeAdmissionCompletePages=config.GetValue<bool>("HemoSeed:IncludeAdmissionCompletePages");
        if(includeCompanyMenuPilot||includeAdmissionReadPages||includeAdmissionCompletePages||config.GetValue<bool>("HemoSeed:IncludeAdmissionRouteMenus"))
            await HemoAdmissionRouteSeed.ApplyAsync(db,menu.GUID,apis,accounts,ct,includeCompanyMenuPilot,includeAdmissionReadPages,includeAdmissionCompletePages);
        var includeRequestFormPages=config.GetValue<bool>("HemoSeed:IncludeRequestFormPages");
        var includeRequestDetailPages=config.GetValue<bool>("HemoSeed:IncludeRequestDetailPages");
        if(includeRequests&&(includeRequestFormPages||includeRequestDetailPages||config.GetValue<bool>("HemoSeed:IncludeRequestReadPages")))
            await HemoRequestReadRouteSeed.ApplyAsync(db,requestMenu!,apis,accounts,ct,includeRequestDetailPages,includeRequestFormPages);
        var includeBankUnitPages=config.GetValue<bool>("HemoSeed:IncludeBankUnitPages");
        var includeBankCompletePages=config.GetValue<bool>("HemoSeed:IncludeBankCompletePages");
        if(includeBank&&(includeBankCompletePages||includeBankUnitPages||config.GetValue<bool>("HemoSeed:IncludeBankReadPages")))
            await HemoBankReadRouteSeed.ApplyAsync(db,bankMenu!,apis,accounts,ct,includeBankUnitPages,includeBankCompletePages);
        var includeTransferOperationPages=config.GetValue<bool>("HemoSeed:IncludeTransferOperationPages");
        if(includeTransfusion&&(includeTransferOperationPages||config.GetValue<bool>("HemoSeed:IncludeTransferReadPages")))
            await HemoTransferReadRouteSeed.ApplyAsync(db,transferMenu!,apis,accounts,ct,includeTransferOperationPages);
        var includeHvReportPages=config.GetValue<bool>("HemoSeed:IncludeHvReportPages");
        var includeHvOperationPages=includeHvReportPages||config.GetValue<bool>("HemoSeed:IncludeHvOperationPages");
        if(includeHv&&(includeHvOperationPages||config.GetValue<bool>("HemoSeed:IncludeHvReadPages")))
            await HemoHvReadRouteSeed.ApplyAsync(db,hvMenu!,apis,accounts,ct,includeHvOperationPages,includeHvReportPages);
        if(includeSamples)await HemoTransfusionSampleSeed.ApplyAsync(db,org.GUID,ct);
        if(includeHvSamples)await HemoHvSampleSeed.ApplyAsync(db,org.GUID,ct);
        if(includeQuality&&config.GetValue<bool>("HemoSeed:IncludeQualityReadPages"))
            await HemoQualityReadRouteSeed.ApplyAsync(db,qualityMenu!,apis,accounts,ct);
        if(includeQualitySamples)await HemoQualitySampleSeed.ApplyAsync(db,org.GUID,ct);
        if(includeDashboardSamples)await HemoDashboardSampleSeed.ApplyAsync(db,org.GUID,ct);
        await tx.CommitAsync(ct);
    }
}
