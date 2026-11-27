namespace Application.Core.BloodRequest_core;

public class RequestLineValidator : AbstractValidator<RequestLineInput>
{
    public RequestLineValidator()
    {
        RuleFor(x=>x.ProductCode).Must(x=>BloodRequestWorkflow.ProductCodes.Contains(x));
        RuleFor(x=>x.Quantity).InclusiveBetween(1,10000);
        RuleFor(x=>x.RequestedBloodGroup).Must(x=>string.IsNullOrEmpty(x)||new[]{"A+","A-","B+","B-","AB+","AB-","O+","O-"}.Contains(x));
        RuleFor(x=>x.SpecialInstructions).MaximumLength(1000);
    }
}
public class ClinicalInputValidator : AbstractValidator<ClinicalInput>
{
    public ClinicalInputValidator()
    {
        RuleFor(x=>x.Diagnosis).MaximumLength(500);RuleFor(x=>x.ClinicalIndication).MaximumLength(2000);
        RuleFor(x=>x.Hemoglobin).InclusiveBetween(0m,999999.99m);RuleFor(x=>x.Hematocrit).InclusiveBetween(0m,100m);
        RuleFor(x=>x.PlateletCount).GreaterThanOrEqualTo(0);RuleFor(x=>x.Premedication).MaximumLength(500);
        RuleFor(x=>x.SampleStatus).Must(x=>x==null||new[]{"Unknown","Collected","Sent","NotSent"}.Contains(x));
        RuleFor(x=>x.Notes).MaximumLength(2000);
        RuleFor(x=>x.FormDetails).SetValidator(new SpecialFormValidator()!).When(x=>x.FormDetails!=null);
    }
}
public class SpecialFormValidator : AbstractValidator<SpecialFormData>
{
    public SpecialFormValidator()
    {
        RuleFor(x=>x.SpecialReason).MaximumLength(2000);RuleFor(x=>x.ReactionDescription).MaximumLength(1000);
        RuleFor(x=>x.PremedicationRoute).MaximumLength(500);RuleFor(x=>x.SampleCollector).MaximumLength(200);
        RuleFor(x=>x.Surgery).MaximumLength(500);RuleFor(x=>x.MsboS).MaximumLength(100);RuleFor(x=>x.AdministrationPlan).MaximumLength(1000);
        RuleFor(x=>x.Pt).InclusiveBetween(0m,999999.99m);RuleFor(x=>x.Ptt).InclusiveBetween(0m,999999.99m);
        RuleFor(x=>x.Inr).InclusiveBetween(0m,999999.99m);RuleFor(x=>x.Fibrinogen).InclusiveBetween(0m,999999.99m);
        RuleFor(x=>x.BleedingSite).MaximumLength(500);RuleFor(x=>x.Refractoriness).MaximumLength(500);
        RuleFor(x=>x.HlaHpa).MaximumLength(500);RuleFor(x=>x.DonorReference).MaximumLength(500);
        RuleFor(x=>x.OriginHospital).MaximumLength(200);RuleFor(x=>x.DestinationHospital).MaximumLength(200);
        RuleFor(x=>x.TransportTemperature).InclusiveBetween(-273.15m,999m);RuleFor(x=>x.TransferMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x=>x.ColdChain).Must(x=>x==null||new[]{"Unknown","ReportedIntact","Suspected","Interrupted"}.Contains(x));
        RuleFor(x=>x.DonationIntervalDays).GreaterThanOrEqualTo(0);RuleFor(x=>x.IronTherapy).MaximumLength(500);
        RuleFor(x=>x.SafetyAssessment).MaximumLength(2000);RuleFor(x=>x.Contact).MaximumLength(100);
        RuleFor(x=>x.EmergencyInstructions).MaximumLength(2000);RuleFor(x=>x.SampleTests).MaximumLength(1000);RuleFor(x=>x.PhysicianReference).MaximumLength(200);
    }
}
public class RequestInputValidator : AbstractValidator<RequestInput>
{
    public RequestInputValidator()
    {
        RuleFor(x=>x.PatientAdmissionGuid).NotEmpty();RuleFor(x=>x.ClientRequestId).NotEmpty();
        RuleFor(x=>x.FormType).Must(x=>BloodRequestWorkflow.FormTypes.Contains(x));
        RuleFor(x=>x.Priority).Must(x=>BloodRequestWorkflow.Priorities.Contains(x));
        RuleFor(x=>x.Items).NotNull().Must(x=>x!=null&&x.Count<=BloodRequestWorkflow.ProductCodes.Count&&x.All(i=>i!=null)&&x.Select(i=>i.ProductCode).Distinct().Count()==x.Count);
        RuleForEach(x=>x.Items).NotNull().SetValidator(new RequestLineValidator());
        RuleFor(x=>x.Clinical).NotNull().SetValidator(new ClinicalInputValidator());
    }
}
public class DecisionInputValidator : AbstractValidator<DecisionInput>
{
    public DecisionInputValidator()
    {
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(x=>x?.Length==8);
        RuleFor(x=>x.Action).Must(x=>new[]{"Submit","Cancel","Approve","Return","Reject"}.Contains(x));
        RuleFor(x=>x.Reason).MaximumLength(2000).NotEmpty().When(x=>BloodRequestWorkflow.RequiresReason(x.Action),ApplyConditionTo.CurrentValidator);
        RuleFor(x=>x.Physician).NotNull().When(x=>x.Action=="Approve");
        When(x=>x.Physician!=null,()=>{
            RuleFor(x=>x.Physician!.Diagnosis).NotEmpty().MaximumLength(500);
            RuleFor(x=>x.Physician!.Indication).NotEmpty().MaximumLength(2000);
            RuleFor(x=>x.Physician!.MedicalCouncilNumber).NotEmpty().MaximumLength(64);
            RuleFor(x=>x.Physician!.Notes).MaximumLength(2000);
            RuleFor(x=>x.Physician!.Confirmed).Equal(true);
        });
    }
}
