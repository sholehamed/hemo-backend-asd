namespace Application.Core.BloodRequest_core;

// Software workflow only. Clinical appropriateness and emergency exceptions require approved policy.
public static class BloodRequestWorkflow
{
    public const string Draft="Draft", AwaitingDoctor="AwaitingDoctor", Returned="Returned", AwaitingBank="AwaitingBank", Rejected="Rejected", Cancelled="Cancelled";
    public static readonly IReadOnlyList<string> ProductCodes=Array.AsReadOnly(new[]{"RBC","WashedRBC","IrradiatedRBC","LeukoreducedRBC","Platelet","ApheresisPlatelet","FFP","WholeBlood","Cryoprecipitate","Autologous","WashedLRRBC","IrradiatedLRRBC","IrradiatedPlatelet","IrradiatedApheresisPlatelet","PediatricRBC","ReconstitutedWholeBlood","CryoPoorPlasma"});
    public static readonly IReadOnlyList<string> FormTypes=Array.AsReadOnly(new[]{"General","WashedRBC","Irradiated","Apheresis","Emergency","InterHospital","Autologous","RBC","Platelet","FFP"});
    public static readonly IReadOnlyList<string> Priorities=Array.AsReadOnly(new[]{"Routine","Urgent","Emergency"});
    public static bool CanEdit(string status)=>status is Draft or Returned;
    public static string? Next(string status,string action)=> (status,action) switch
    {
        (Draft or Returned,"Submit")=>AwaitingDoctor,
        (AwaitingDoctor,"Approve")=>AwaitingBank,
        (AwaitingDoctor,"Return")=>Returned,
        (AwaitingDoctor,"Reject")=>Rejected,
        (Draft or Returned or AwaitingDoctor,"Cancel")=>Cancelled,
        _=>null
    };
    public static bool SourceAllowsForm(string source,string form)=>FormTypes.Contains(form)&&
        (source=="Nurse"?form!="Autologous":source=="Doctor"&&form is not ("RBC" or "Platelet" or "FFP" or "InterHospital"));
    public static bool FormMatchesItems(string form,IEnumerable<BloodRequestItem> items)
    {
        var codes=items.Where(i=>!i.IsDelete).Select(i=>i.ProductCode).ToArray();
        return form switch
        {
            "RBC"=>codes.Contains("RBC")||codes.Contains("LeukoreducedRBC")||codes.Contains("PediatricRBC"),
            "WashedRBC"=>codes.Length>0&&codes.All(c=>c is "WashedRBC" or "WashedLRRBC"),
            "Irradiated"=>codes.Length>0&&codes.All(c=>c is "IrradiatedRBC" or "IrradiatedLRRBC" or "IrradiatedPlatelet" or "IrradiatedApheresisPlatelet"),
            "Platelet"=>codes.Contains("Platelet"),
            "Apheresis"=>codes.Contains("ApheresisPlatelet"),
            "FFP"=>codes.Contains("FFP"),
            "Autologous"=>codes.Length>0&&codes.All(c=>c=="Autologous"),
            _=>true
        };
    }
    public static bool RequiresReason(string action)=>action is "Return" or "Reject" or "Cancel";
    public static bool HasValidItems(IEnumerable<BloodRequestItem> items)
    {
        var lines=items.Where(i=>!i.IsDelete).ToArray();
        return lines.Length>0 && lines.All(i=>ProductCodes.Contains(i.ProductCode)&&i.Quantity is >0 and <=10000)
            && lines.Select(i=>i.ProductCode).Distinct(StringComparer.Ordinal).Count()==lines.Length;
    }
}
