namespace Application.Core.BloodBank_core;

public record ReceiveUnit(string UnitCode,string ProductCode,string BloodGroup,string? BatchNumber,string SupplySource,string EquipmentCode,DateTimeOffset ReceivedAt,DateTimeOffset ExpiresAt,string? Notes=null);
public record UnitAction(Guid Guid,byte[] Version,string Action,Guid? RequestGuid=null,byte[]? RequestVersion=null,string? Reason=null,string? Method=null,decimal? Temperature=null,DateTimeOffset? EffectiveExpiry=null);
public record LabInput(Guid UnitGuid,byte[] UnitVersion,Guid RequestGuid,byte[] RequestVersion,string SampleReference,string SecondSample,string LabelGroup,string Method,string ImmediateSpin,string Phase37,string Ahg,string Dat,string AntibodyScreen,string ReportReference,string Conclusion,string? Notes);
public record TemperatureInput(string EquipmentCode,DateTimeOffset MeasuredAt,decimal Temperature,string Assessment,string? Notes);
public record ReturnRequestInput(Guid RequestGuid,byte[] Version,string Reason);
public record UnitRow(Guid Guid,string UnitCode,string ProductCode,string BloodGroup,string? BatchNumber,string SupplySource,string EquipmentCode,DateTime ReceivedAt,DateTime ExpiresAt,string Status,bool Expired,Guid? RequestGuid,DateTime? ThawStartedAt,DateTime? ThawCompletedAt,byte[] Version,bool EquipmentBlocked=false);
public record UnitEventView(string Action,string PreviousStatus,string NewStatus,Guid? RequestGuid,DateTime OccurredAt,Guid ActorUserGuid,Guid ActorRoleGuid,string? Reason,string? Method,decimal? Temperature,DateTime? EffectiveExpiry);
public record LabView(Guid Guid,Guid RequestGuid,int SubmissionAttempt,DateTime RecordedAt,Guid ActorUserGuid,string SampleReference,string SecondSample,string LabelGroup,string Method,string ImmediateSpin,string Phase37,string Ahg,string Dat,string AntibodyScreen,string ReportReference,string Conclusion,string? Notes);
public record TemperatureView(Guid Guid,string EquipmentCode,DateTime MeasuredAt,DateTime RecordedAt,decimal Temperature,string Assessment,string? Notes,Guid ActorUserGuid);
public record UnitDetail(UnitRow Unit,List<UnitEventView> Events,List<LabView> Tests,TemperatureView? LatestTemperature);
public record BankCapabilities(bool Inventory,bool Unit,bool Stats,bool TemperatureHistory,bool Receive,bool Inspect,bool Test,bool Reserve,bool Thaw,bool Ready,bool Hold,bool Unreserve,bool Discard,bool Temperature,bool ReturnRequest,bool LabHistory);
public record BankStats(int Total,int Available,int Reserved,int Thawing,int Ready,int Hold,int Expired,int ExpiringSoon,int Discarded,int ApprovedRequests,int TemperatureConcerns,List<BankStockCell> Stock,int EquipmentBlocked=0);
public record BankStockCell(string ProductCode,string BloodGroup,int Available);
public record BankLabReportRow(string UnitCode,string ProductCode,string BloodGroup,string RequestNumber,LabView Test);
