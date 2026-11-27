namespace Application.Core.BloodRequest_core;

// Software workflow only. Clinical appropriateness and emergency exceptions require approved policy.
public static class BloodRequestWorkflow
{
    public const string Draft="Draft", AwaitingDoctor="AwaitingDoctor", Returned="Returned", AwaitingBank="AwaitingBank", Rejected="Rejected", Cancelled="Cancelled";
    public static readonly IReadOnlyList<string> ProductCodes=Array.AsReadOnly(new[]{"RBC","WashedRBC","IrradiatedRBC","LeukoreducedRBC","Platelet","ApheresisPlatelet","FFP","WholeBlood","Cryoprecipitate","Autologous"});
    public static readonly IReadOnlyList<string> FormTypes=Array.AsReadOnly(new[]{"General","WashedRBC","Irradiated","Apheresis","Emergency","InterHospital","Autologous"});
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
    public static bool RequiresReason(string action)=>action is "Return" or "Reject" or "Cancel";
    public static bool HasValidItems(IEnumerable<BloodRequestItem> items)
    {
        var lines=items.Where(i=>!i.IsDelete).ToArray();
        return lines.Length>0 && lines.All(i=>ProductCodes.Contains(i.ProductCode)&&i.Quantity>0)
            && lines.Select(i=>i.ProductCode).Distinct(StringComparer.Ordinal).Count()==lines.Length;
    }
}
