namespace Application.Core.BloodRequest_core;

public record RequestLineInput(string ProductCode,int Quantity,string? RequestedBloodGroup=null,string? SpecialInstructions=null);
public record SpecialFormData(string? SpecialReason=null,string? ReactionDescription=null,string? PremedicationRoute=null,
    string? SampleCollector=null,DateTime? SampleAt=null,DateTime? NeededAt=null,
    string? Surgery=null,DateTime? SurgeryAt=null,string? MsboS=null,string? AdministrationPlan=null,
    decimal? Pt=null,decimal? Ptt=null,decimal? Inr=null,decimal? Fibrinogen=null,
    string? BleedingSite=null,string? Refractoriness=null,string? HlaHpa=null,string? DonorReference=null,
    string? OriginHospital=null,string? DestinationHospital=null,decimal? TransportTemperature=null,int? TransferMinutes=null,string? ColdChain=null,
    int? DonationIntervalDays=null,string? IronTherapy=null,bool? ConsentDocumented=null,string? SafetyAssessment=null,string? Contact=null,
    string? EmergencyInstructions=null,string? SampleTests=null,string? PhysicianReference=null);
public record ClinicalInput(string? Diagnosis=null,string? ClinicalIndication=null,decimal? Hemoglobin=null,decimal? Hematocrit=null,
    int? PlateletCount=null,bool? RecentTransfusion=null,bool? RecentPregnancy=null,bool? PriorReaction=null,bool? UnexpectedAntibody=null,
    string? Premedication=null,string? SampleStatus=null,string? Notes=null,SpecialFormData? FormDetails=null);
public record RequestInput(Guid PatientAdmissionGuid,Guid ClientRequestId,string FormType,string Priority,List<RequestLineInput> Items,ClinicalInput Clinical);
public record PhysicianCompletion(string Diagnosis,string Indication,string MedicalCouncilNumber,string? Notes=null,bool Confirmed=false);
public record DecisionInput(Guid Guid,byte[] Version,string Action,string? Reason=null,PhysicianCompletion? Physician=null);
public record RequestRow(Guid Guid,string RequestNumber,Guid PatientAdmissionGuid,string PatientName,string HisNumber,string Ward,
    string Source,string FormType,string Priority,string Status,DateTime Created,DateTime? SubmittedAt,int TotalUnits,bool Owned);
public record ApprovalView(string Action,string PreviousStatus,string NewStatus,string? Reason,int SubmissionAttempt,DateTime OccurredAt,Guid ActorUserGuid,Guid ActorRoleGuid,PhysicianCompletion? Physician);
public record RequestDetail(RequestRow Request,byte[] Version,string? BloodGroup,string IdentityStatus,string BedNumber,
    List<RequestLineInput> Items,ClinicalInput Clinical,List<ApprovalView> History,bool AdmissionChanged);
public record RequestCapabilities(bool List,bool Detail,bool NurseCreate,bool DoctorCreate,bool Edit,bool Submit,bool Cancel,bool Decide,bool BankQueue,bool BankDetail);
public record RequestStats(int Total,int Draft,int AwaitingDoctor,int Returned,int AwaitingBank,int Rejected,int Cancelled);
