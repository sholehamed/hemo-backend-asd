using Application.Core.Quality_core;
using Domain.Entities;
using Xunit;
namespace Hemo.Admission.Tests;
public class QualityRulesTests
{
    [Fact] public void HoldAndOverdueBlockEquipment(){var now=DateTime.UtcNow;Assert.False(QualityRules.CanOperate(new HemoEquipment{Status="Hold"},now));Assert.False(QualityRules.CanOperate(new HemoEquipment{Status="Operational",CalibrationDueAt=now},now));Assert.True(QualityRules.CanOperate(new HemoEquipment{Status="Operational",CalibrationDueAt=now.AddDays(1)},now));}
    [Theory][InlineData("Hold",false,true,true)][InlineData("Hold",true,true,false)][InlineData("Hold",false,false,false)][InlineData("Archived",false,true,false)]
    public void ReleaseRequiresRecordedChecks(string s,bool i,bool d,bool expected)=>Assert.Equal(expected,QualityRules.CanRelease(s,i,d));
    [Fact]public void ArchiveCannotHaveStock(){Assert.False(QualityRules.CanArchive("Hold",true,false));Assert.True(QualityRules.CanArchive("Hold",false,false));}
    [Theory][InlineData("Draft","Submit","Submitted")][InlineData("Submitted","Approve","Approved")][InlineData("Submitted","Reject","Draft")][InlineData("Approved","Close","Closed")][InlineData("Closed","Submit",null)]
    public void ReviewFlow(string s,string a,string? n)=>Assert.Equal(n,QualityRules.Next(s,a));
}
