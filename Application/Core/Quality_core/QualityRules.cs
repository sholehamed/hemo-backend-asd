namespace Application.Core.Quality_core;
public static class QualityRules
{
    public static readonly string[] Categories=["Refrigerator","Freezer","Incubator","Centrifuge","WaterBath","Analyzer","Transport","Other"];
    public static readonly string[] ServiceKinds=["Calibration","Maintenance","Repair","Qualification"];
    public static readonly string[] Urgencies=["Routine","Urgent","Critical"];
    public static readonly string[] Kinds=["Qc","Committee","Audit","Training"];
    public static bool Text(string? value,int max)=>!string.IsNullOrWhiteSpace(value)&&value.Length<=max;
    public static bool Optional(string? value,int max)=>value==null||value.Length<=max;
    public static bool Version(byte[]? v)=>v?.Length==8;
    public static bool CanOperate(HemoEquipment e,DateTime now)=>e.Status=="Operational"&&(!e.CalibrationDueAt.HasValue||e.CalibrationDueAt>now)&&(!e.MaintenanceDueAt.HasValue||e.MaintenanceDueAt>now);
    public static bool CanRelease(string state,bool incident,bool documented)=>state=="Hold"&&!incident&&documented;
    public static bool CanArchive(string state,bool hasStock,bool incident)=>state=="Hold"&&!hasStock&&!incident;
    public static bool ValidScore(int? n)=>n.HasValue&&n.Value>=0&&n.Value<=20;
    public static bool Independent(Guid actor,Guid creator)=>actor!=creator;
    public static string? Next(string status,string action)=>(status,action) switch
    {
        ("Draft","Submit")=>"Submitted",("Submitted","Approve")=>"Approved",("Submitted","Reject")=>"Draft",("Approved","Close")=>"Closed",_=>null
    };
}
