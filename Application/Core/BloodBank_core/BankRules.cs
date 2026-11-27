using Application.Core.BloodRequest_core;
namespace Application.Core.BloodBank_core;

public static class BankRules
{
    public const string ReceivedHold="ReceivedHold", Available="Available", Reserved="Reserved", Thawing="Thawing", Ready="Ready", Quarantined="Quarantined", Discarded="Discarded";
    public static readonly string[] Groups=["A+","A-","B+","B-","AB+","AB-","O+","O-"];
    public static readonly string[] Results=["NonReactive","Reactive","NotPerformed","Indeterminate"];
    public static readonly string[] Assessments=["Acceptable","Concern","Unknown"];
    public static bool NeedsThaw(string product)=>product is "FFP" or "Cryoprecipitate" or "CryoPoorPlasma";
    public static bool IsExpired(DateTime expiry,DateTime now)=>expiry<=now;
    public static bool CurrentApproval(BloodRequest request)=>request.Status==BloodRequestWorkflow.AwaitingBank &&
        request.PatientAdmission.Status=="Active" && request.PatientAdmission.IdentityStatus=="Verified" &&
        request.AdmissionVersionAtSubmission!=null && request.AdmissionVersionAtSubmission.SequenceEqual(request.PatientAdmission.Version) &&
        request.Approvals.Any(a=>a.Action=="Approve"&&a.SubmissionAttempt==request.SubmissionAttempt&&a.NewStatus==BloodRequestWorkflow.AwaitingBank);
    public static string? Next(string status,string action,bool needsThaw,bool thawComplete)=>(status,action) switch
    {
        (ReceivedHold or Quarantined,"Inspect")=>Available,
        (Available,"Reserve")=>Reserved,
        (Reserved,"StartThaw") when needsThaw&&!thawComplete=>Thawing,
        (Thawing,"FinishThaw")=>Reserved,
        (Reserved,"Ready") when !needsThaw||thawComplete=>Ready,
        (Available or Reserved or Ready or Thawing or ReceivedHold,"Hold")=>Quarantined,
        (Reserved or Ready or Thawing or Quarantined,"Unreserve")=>Quarantined,
        (ReceivedHold or Quarantined,"Discard")=>Discarded,
        _=>null
    };
    public static bool AcceptedLab(BankCompatibilityTest test)=>test.Conclusion=="Accepted"&&test.SecondSample=="Confirmed"&&
        test.ImmediateSpin is "NonReactive" or "NotPerformed" && test.Phase37 is "NonReactive" or "NotPerformed" &&
        test.Ahg is "NonReactive" or "NotPerformed" &&
        test.Dat!="Indeterminate"&&test.AntibodyScreen!="Indeterminate"&&
        (!(new[]{test.ImmediateSpin,test.Phase37,test.Ahg,test.Dat,test.AntibodyScreen}.Contains("NotPerformed")||test.Dat=="Reactive"||test.AntibodyScreen=="Reactive")||!string.IsNullOrWhiteSpace(test.Notes));
    public static bool LabCurrent(BankCompatibilityTest test,BloodRequest request,BankBloodUnit unit)=>AcceptedLab(test)&&
        test.SubmissionAttempt==request.SubmissionAttempt&&test.RequestVersion!=null&&test.RequestVersion.SequenceEqual(request.Version)&&
        test.AdmissionVersion!=null&&test.AdmissionVersion.SequenceEqual(request.PatientAdmission.Version)&&
        unit.InspectionAt.HasValue&&test.RecordedAt>=unit.InspectionAt.Value&&test.LabelGroup==unit.BloodGroup;
}
