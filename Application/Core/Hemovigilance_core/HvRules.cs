namespace Application.Core.Hemovigilance_core;
public static class HvRules
{
    public static readonly string[] Symptoms=["Fever","Chills","Urticaria","Pruritus","Dyspnea","Hypotension","Hypertension","Tachycardia","Nausea","Vomiting","ChestPain","BackPain","Hemoglobinuria","Cyanosis","Jaundice","Other","Unknown"];
    // Recorded classifications only; no diagnosis or severity inference.
    public static readonly string[] Diagnoses=["Undetermined","FNHTR","Allergic","Anaphylaxis","AHTR","DHTR","TACO","TRALI","TAD","Septic","Hypotensive","PTP","TAGVHD","Other"];
    public static readonly string[] Severities=["Unknown","Grade1","Grade2","Grade3","Grade4"];
    public static readonly string[] Imputabilities=["Unknown","Excluded","Unlikely","Possible","Probable","Certain"];
    public static readonly string[] Outcomes=["Unknown","Recovered","Recovering","Ongoing","Sequelae","Death","Other"];
    public static readonly string[] ExternalStatuses=["NotRecorded","NotRequired","Required","SentManually"];
    public static readonly string[] Methods=["FiveWhys","Fishbone","Timeline","Other"];
    public static bool CanReport(BloodTransfer t)=>t.StartedAt.HasValue;
    public static string? Next(string status,string action,bool rca=false)=>(status,action) switch
    {
        ("Reported","DoctorReview")=>"DoctorReviewed",
        ("DoctorReviewed","BankReview")=>"BankReviewed",
        ("BankReviewed","Investigate")=>rca?"RcaOpen":"ReadyToClose",
        ("RcaOpen","PublishRca")=>"CapaMonitoring",
        ("ReadyToClose" or "CapaMonitoring","Close")=>"Closed",
        ("DoctorReviewed" or "BankReviewed" or "RcaOpen" or "ReadyToClose","ReturnToDoctor")=>"Reported",
        _=>null
    };
    public static bool CloseAllowed(HemoReaction r,IEnumerable<HemoCapa> actions)=>r.Status=="ReadyToClose"&&r.RcaRequired==false||r.Status=="CapaMonitoring"&&r.RcaRequired==true&&r.RcaPublishedAt.HasValue&&actions.Any()&&actions.All(x=>x.Status=="Verified");
    public static bool Independent(Guid actor,Guid owner,Guid? submitter)=>actor!=owner&&submitter.HasValue&&actor!=submitter.Value;
    public static bool ExternalComplete(string status,string? reference)=>status=="NotRequired"||status=="SentManually"&&!string.IsNullOrWhiteSpace(reference);
}
