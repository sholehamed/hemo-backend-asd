using Application.Core.BloodBank_core;
namespace Application.Core.Transfusion_core;
// Workflow gates record documented observations; no automated clinical judgement.
public static class TransferRules
{
    public const string Dispatched="Dispatched",Received="Received",ReceiptHold="ReceiptHold",Verified="Verified",Witnessed="Witnessed",InProgress="InProgress",Completed="Completed",Stopped="Stopped",Returning="Returning",Returned="Returned",Disposed="Disposed";
    public const int RequiredChecks=31;
    public static readonly string[] Phases=["T0","T15","Mid","End","Post1h"];
    public static string? Next(string status,string action)=>(status,action) switch
    {
        (Dispatched,"Receive")=>Received,
        (Dispatched,"ReceiptHold")=>ReceiptHold,
        (Received or Verified or Witnessed,"Verify")=>Verified,
        (Verified,"Witness")=>Witnessed,
        (Witnessed,"Start")=>InProgress,
        (InProgress,"Complete")=>Completed,
        (InProgress,"Stop")=>Stopped,
        (Dispatched or Received or ReceiptHold or Verified or Witnessed or Stopped,"Return")=>Returning,
        (Returning,"AcceptReturn")=>Returned,
        (Returned,"Dispose")=>Disposed,
        _=>null
    };
    public static bool VitalAllowed(string status,string phase)=>phase switch
    {
        "T0"=>status is Verified or Witnessed,
        "T15" or "Mid" or "End"=>status==InProgress,
        "Post1h"=>status is Completed or Stopped or Returning or Returned or Disposed,
        _=>false
    };
    public static bool DistinctWitness(Guid? verifier,Guid witness)=>verifier.HasValue&&verifier.Value!=Guid.Empty&&witness!=Guid.Empty&&verifier.Value!=witness;
    public static bool ExactCodes(string? unit,string? band,BloodTransfer t)=>string.Equals(unit?.Trim(),t.UnitCode,StringComparison.Ordinal)&&string.Equals(band?.Trim(),t.WristbandCode,StringComparison.Ordinal);
    public static bool Current(BloodTransfer t,BloodRequest r,BankBloodUnit u,BankCompatibilityTest test,DateTime now)=>
        BankRules.CurrentApproval(r)&&!BankRules.IsExpired(u.ExpiresAt,now)&&t.RequestVersion!=null&&t.RequestVersion.SequenceEqual(r.Version)&&
        t.AdmissionVersion!=null&&t.AdmissionVersion.SequenceEqual(r.PatientAdmission.Version)&&t.PatientAdmissionGUID==r.PatientAdmissionGUID&&
        t.BankBloodUnitGUID==u.GUID&&t.BloodRequestGUID==r.GUID&&u.BloodRequestGUID==r.GUID&&t.BankCompatibilityTestGUID==test.GUID&&
        t.Ward==r.PatientAdmission.Ward&&t.WristbandCode==r.PatientAdmission.WristbandCode&&t.UnitCode==u.UnitCode&&BankRules.LabCurrent(test,r,u);
}
