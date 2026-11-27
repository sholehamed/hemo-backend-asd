using Application.Core.BloodRequest_core;
namespace Application.Core.BloodBank_core;
public static class BankValidation
{
    public static bool Code(string? value,int max=80)=>!string.IsNullOrWhiteSpace(value)&&value.Length<=max;
    public static bool Version(byte[]? value)=>value?.Length==8;
    public static string Normalize(string value)=>value.Trim().ToUpperInvariant();
    public static bool Temperature(decimal? value)=>value.HasValue&&value>=-273.15m&&value<=1000m;
}
public class ReceiveUnitValidator:AbstractValidator<ReceiveUnit>
{
    public ReceiveUnitValidator(){
        RuleFor(x=>x.UnitCode).Must(x=>BankValidation.Code(x));RuleFor(x=>x.ProductCode).Must(x=>BloodRequestWorkflow.ProductCodes.Contains(x));RuleFor(x=>x.BloodGroup).Must(x=>BankRules.Groups.Contains(x));
        RuleFor(x=>x.BatchNumber).MaximumLength(80);RuleFor(x=>x.SupplySource).NotEmpty().MaximumLength(200);RuleFor(x=>x.EquipmentCode).Must(x=>BankValidation.Code(x));
        RuleFor(x=>x.ReceivedAt).NotEmpty();RuleFor(x=>x.ExpiresAt).GreaterThan(x=>x.ReceivedAt);RuleFor(x=>x.Notes).MaximumLength(2000);
    }
}
public class UnitActionValidator:AbstractValidator<UnitAction>
{
    public UnitActionValidator(){
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(BankValidation.Version);
        RuleFor(x=>x.Action).Must(x=>new[]{"Inspect","Reserve","StartThaw","FinishThaw","Ready","Hold","Unreserve","Discard"}.Contains(x));
        RuleFor(x=>x.Reason).MaximumLength(2000).NotEmpty();RuleFor(x=>x.Method).MaximumLength(100);
        RuleFor(x=>x.Temperature).Must(x=>x==null||BankValidation.Temperature(x));
        When(x=>x.Action=="Reserve",()=>{RuleFor(x=>x.RequestGuid).NotEmpty().Must(x=>x.HasValue&&x.Value!=Guid.Empty);RuleFor(x=>x.RequestVersion).Must(BankValidation.Version);});
        When(x=>x.Action is "StartThaw" or "FinishThaw",()=>RuleFor(x=>x.Method).NotEmpty());
        When(x=>x.Action=="FinishThaw",()=>{RuleFor(x=>x.Temperature).Must(BankValidation.Temperature);RuleFor(x=>x.EffectiveExpiry).NotNull();});
    }
}
public class LabInputValidator:AbstractValidator<LabInput>
{
    public LabInputValidator(){
        RuleFor(x=>x.UnitGuid).NotEmpty();RuleFor(x=>x.RequestGuid).NotEmpty();RuleFor(x=>x.UnitVersion).Must(BankValidation.Version);RuleFor(x=>x.RequestVersion).Must(BankValidation.Version);
        RuleFor(x=>x.SampleReference).NotEmpty().MaximumLength(100);RuleFor(x=>x.SecondSample).Must(x=>new[]{"Confirmed","Discrepant","Pending"}.Contains(x));RuleFor(x=>x.LabelGroup).Must(x=>BankRules.Groups.Contains(x));
        RuleFor(x=>x.Method).NotEmpty().MaximumLength(100);RuleFor(x=>x.ReportReference).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.ImmediateSpin).Must(x=>BankRules.Results.Contains(x));RuleFor(x=>x.Phase37).Must(x=>BankRules.Results.Contains(x));RuleFor(x=>x.Ahg).Must(x=>BankRules.Results.Contains(x));RuleFor(x=>x.Dat).Must(x=>BankRules.Results.Contains(x));RuleFor(x=>x.AntibodyScreen).Must(x=>BankRules.Results.Contains(x));
        RuleFor(x=>x.Conclusion).Must(x=>new[]{"Accepted","Rejected","Pending"}.Contains(x));RuleFor(x=>x.Notes).MaximumLength(2000);RuleFor(x=>x.Notes).NotEmpty().When(x=>x.Conclusion!="Accepted");
    }
}
public class TemperatureInputValidator:AbstractValidator<TemperatureInput>
{
    public TemperatureInputValidator(){
        RuleFor(x=>x.EquipmentCode).Must(x=>BankValidation.Code(x));RuleFor(x=>x.MeasuredAt).NotEmpty();RuleFor(x=>x.Temperature).Must(x=>BankValidation.Temperature(x));RuleFor(x=>x.Assessment).Must(x=>BankRules.Assessments.Contains(x));RuleFor(x=>x.Notes).MaximumLength(2000);RuleFor(x=>x.Notes).NotEmpty().When(x=>x.Assessment!="Acceptable");
    }
}
public class ReturnRequestInputValidator:AbstractValidator<ReturnRequestInput>
{
    public ReturnRequestInputValidator(){RuleFor(x=>x.RequestGuid).NotEmpty();RuleFor(x=>x.Version).Must(BankValidation.Version);RuleFor(x=>x.Reason).NotEmpty().MaximumLength(2000);}
}
