using Application.Core.Quality_core;
using Xunit;
namespace Hemo.Admission.Tests;
public class QualityValidationTests
{
    [Fact] public void PendingQcCannotSubmit(){var h=new QualityHeader(Shift:"Morning",Operator:"Fixture");Assert.False(QualityValidation.CanSubmit("Qc",h,new[]{new QualityLineData(Result:"Pending")},DateTime.UtcNow.AddMinutes(-1)));Assert.False(QualityValidation.CanSubmit("Qc",h,new[]{new QualityLineData(Result:"Rejected")},DateTime.UtcNow.AddMinutes(-1)));Assert.True(QualityValidation.CanSubmit("Qc",h with{Nonconformance="Fixture",CorrectiveAction="Fixture"},new[]{new QualityLineData(Result:"Rejected")},DateTime.UtcNow.AddMinutes(-1)));}
    [Fact] public void AcceptedExpiredReagentCannotBeRecorded(){var d=new QualityLineData(Lot:"DEMO",Manufacturer:"Fixture",Expiry:DateTimeOffset.UtcNow.AddDays(-1),PositiveExpected:"Fixture",NegativeExpected:"Fixture",PositiveObserved:"Fixture",NegativeObserved:"Fixture",Result:"Accepted");Assert.False(QualityValidation.Line("Qc","Fixture",d,null,null,DateTime.UtcNow));}
    [Fact] public void AuditDeficiencyNeedsNotes(){Assert.False(QualityValidation.Line("Audit","Fixture",new QualityLineData(Clause:"1",Score:0),null,null,DateTime.UtcNow));Assert.True(QualityValidation.Line("Audit","Fixture",new QualityLineData(Clause:"1",Score:0,Notes:"Fixture"),null,null,DateTime.UtcNow));}
    [Fact] public void TrainingCannotSubmitUnresolvedAttendance(){Assert.False(QualityValidation.CanSubmit("Training",new QualityHeader(Capacity:1),new[]{new QualityLineData(Attendance:"Registered")},DateTime.UtcNow.AddMinutes(-1)));}
    [Fact] public void AbsentPersonCannotReceiveScoreOrCertificate(){Assert.False(QualityValidation.Line("Training","Fixture",new QualityLineData(PersonnelCode:"Fixture",Ward:"Fixture",Shift:"Morning",Attendance:"Absent",PostScore:10),Guid.NewGuid(),null,DateTime.UtcNow));}
}
