namespace Application.Core.Quality_core;
// Typed input and persisted JSON payloads; clients cannot submit arbitrary JSON or a status.
public record QualityHeader(string? Chair=null,string? Secretary=null,string? Participants=null,string? Agenda=null,string? Shift=null,string? Operator=null,decimal? WaterBathTemperature=null,string? Nonconformance=null,string? CorrectiveAction=null,string? Checklist=null,string? Auditor=null,string? Instructor=null,int? DurationMinutes=null,int? Capacity=null,string? Audience=null,string? MaterialReference=null);
public record QualityLineData(string? Lot=null,string? Manufacturer=null,DateTimeOffset? Expiry=null,string? PositiveExpected=null,string? NegativeExpected=null,string? PositiveObserved=null,string? NegativeObserved=null,string? Result=null,string? Clause=null,int? Score=null,string? Notes=null,string? Indicator=null,string? PersonnelCode=null,string? Ward=null,string? Shift=null,string? Attendance=null,int? PreScore=null,int? PostScore=null,string? CertificateReference=null);
public record QualityCreate(string Title,string Ward,DateTimeOffset OccurredAt,QualityHeader Header);
public record QualityEdit(Guid Guid,byte[] Version,string Title,string Ward,DateTimeOffset OccurredAt,QualityHeader Header,string Notes);
public record QualityLineInput(Guid Guid,byte[] Version,string Title,QualityLineData Data,Guid? OwnerUserGuid=null,DateTimeOffset? DueAt=null,Guid? LineGuid=null,byte[]? LineVersion=null);
public record QualityAction(Guid Guid,byte[] Version,string Notes);
public record ResolutionProgress(Guid Guid,byte[] Version,Guid LineGuid,byte[] LineVersion,int Progress,string Evidence);
public record QualityFilter(string? Search=null,string? Ward=null,string? Status=null,DateTimeOffset? From=null,DateTimeOffset? To=null,int PageNumber=1,int PageSize=20,bool Mine=false);
public record QualityRow(Guid Guid,string Kind,string Number,string Title,string Ward,string Status,DateTime OccurredAt,Guid CreatedByUserGuid,DateTime? ReviewedAt,byte[] Version,bool Demo);
public record QualityLineView(Guid Guid,string Title,QualityLineData Data,string Status,Guid? OwnerUserGuid,DateTime? DueAt,int Progress,string? Evidence,Guid? VerifiedBy,DateTime? VerifiedAt,byte[] Version);
public record QualityEventView(string Action,string PreviousStatus,string NewStatus,DateTime OccurredAt,Guid ActorUserGuid,Guid ActorRoleGuid,string Notes,string? EvidenceJson);
public record QualityDetail(QualityRow Record,QualityHeader Header,List<QualityLineView> Lines,List<QualityEventView> Events,string? ReviewNotes,Guid? ReviewedBy,int? AuditScore,int? AuditMaximum);
public record QualityOwner(Guid UserGuid,string Name);
public record FacilityInput(byte[]? Version,string FacilityName,string? AccreditationCode,string? Chair,string? Secretary,string? Officer,string? BloodCenter,string? BloodCenterCode,string? ReportFooter,string Notes);
public record FacilityEventView(DateTime OccurredAt,Guid ActorUserGuid,string Notes,string? PreviousJson,string CurrentJson);
public record FacilityView(string FacilityName,string? AccreditationCode,string? Chair,string? Secretary,string? Officer,string? BloodCenter,string? BloodCenterCode,string? ReportFooter,byte[]? Version,List<FacilityEventView>? History=null);
public record QualityStatsFilter(DateTimeOffset? From=null,DateTimeOffset? To=null,string? Ward=null);
public record QualityCount(string Key,int Count);
public record QualityUsage(string ProductCode,int Received,int Started,int Completed,int Discarded);
public record QualityStats(int QcRecords,int QcAwaitingReview,int CommitteeMeetings,int OpenResolutions,int OverdueResolutions,int Audits,int TrainingCourses,int AttendanceRecords,int Present,int EquipmentIncidents,int ReactionCases,List<QualityUsage> Products,List<QualityCount> CompletedByWard);

public record QualityCapabilities(Dictionary<string,bool> Permissions,Guid UserGuid);
