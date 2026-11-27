namespace Application.Core.Transfusion_core;
public class DispatchInputValidator:AbstractValidator<DispatchInput>
{
    public DispatchInputValidator(){
        RuleFor(x=>x.UnitGuid).NotEmpty();RuleFor(x=>x.UnitVersion).Must(v=>v?.Length==8);
        RuleFor(x=>x.UnitCode).NotEmpty().MaximumLength(80);RuleFor(x=>x.WristbandCode).NotEmpty().MaximumLength(80);
        RuleFor(x=>x.CarrierName).NotEmpty().MaximumLength(160);RuleFor(x=>x.CarrierCode).NotEmpty().MaximumLength(80);
        RuleFor(x=>x.ContainerCode).NotEmpty().MaximumLength(100);RuleFor(x=>x.Temperature).InclusiveBetween(-273.15m,1000m);
        RuleFor(x=>x.TransportIndicator).NotEmpty().MaximumLength(100);RuleFor(x=>x.PackCount).Must(x=>x==null||x>=0&&x<=1000);
        RuleFor(x=>x.Checks).Equal(TransferRules.RequiredChecks);RuleFor(x=>x.Notes).NotEmpty().MaximumLength(2000);
    }
}
public class TransferActionValidator:AbstractValidator<TransferActionInput>
{
    public TransferActionValidator(){
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(v=>v?.Length==8);
        RuleFor(x=>x.UnitCode).MaximumLength(80);RuleFor(x=>x.WristbandCode).MaximumLength(80);
        RuleFor(x=>x.Notes).NotEmpty().MaximumLength(2000);RuleFor(x=>x.Temperature).Must(x=>x==null||x>=-273.15m&&x<=1000m);
        RuleFor(x=>x.VolumeMl).Must(x=>x==null||x>=0&&x<=1000000);RuleFor(x=>x.Condition).MaximumLength(100);RuleFor(x=>x.ReceiverName).MaximumLength(160);RuleFor(x=>x.NeedleReference).MaximumLength(100);
    }
}
public class VitalInputValidator:AbstractValidator<VitalInput>
{
    // Representation bounds only; these are NOT normal clinical reference ranges.
    public VitalInputValidator(){
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(v=>v?.Length==8);RuleFor(x=>x.Phase).Must(x=>TransferRules.Phases.Contains(x));
        RuleFor(x=>x.MeasuredAt).NotEmpty();RuleFor(x=>x.Systolic).InclusiveBetween(0,1000);RuleFor(x=>x.Diastolic).InclusiveBetween(0,1000);
        RuleFor(x=>x.Pulse).InclusiveBetween(0,1000);RuleFor(x=>x.Temperature).InclusiveBetween(-273.15m,1000m);
        RuleFor(x=>x.Spo2).InclusiveBetween(0,100);RuleFor(x=>x.Respiration).InclusiveBetween(0,1000);
        RuleFor(x=>x.RateMlPerHour).Must(x=>x==null||x>=0&&x<=1000000);RuleFor(x=>x.UrineMl).Must(x=>x==null||x>=0&&x<=1000000);RuleFor(x=>x.Notes).MaximumLength(2000);
    }
}
