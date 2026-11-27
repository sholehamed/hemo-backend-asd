using Application.Core.Hemovigilance_core;
using Domain.Entities;
using Xunit;
namespace Hemo.Admission.Tests;
public class HvRulesTests
{
    [Theory]
    [InlineData("Reported","DoctorReview",false,"DoctorReviewed")]
    [InlineData("Reported","Close",false,null)]
    [InlineData("DoctorReviewed","BankReview",false,"BankReviewed")]
    [InlineData("BankReviewed","Investigate",true,"RcaOpen")]
    [InlineData("BankReviewed","Investigate",false,"ReadyToClose")]
    [InlineData("RcaOpen","PublishRca",false,"CapaMonitoring")]
    [InlineData("Closed","DoctorReview",false,null)]
    [InlineData("BankReviewed","ReturnToDoctor",false,"Reported")]
    public void Workflow(string s,string a,bool r,string? expected)=>Assert.Equal(expected,HvRules.Next(s,a,r));
    [Fact] public void CloseNeedsVerifiedActions(){var r=new HemoReaction{Status="CapaMonitoring",RcaRequired=true,RcaPublishedAt=DateTime.UtcNow};Assert.False(HvRules.CloseAllowed(r,Array.Empty<HemoCapa>()));Assert.False(HvRules.CloseAllowed(r,new[]{new HemoCapa{Status="Submitted"}}));Assert.True(HvRules.CloseAllowed(r,new[]{new HemoCapa{Status="Verified"}}));}
    [Fact] public void OwnerCannotVerifyOwnAction(){var a=Guid.NewGuid();Assert.False(HvRules.Independent(a,a,a));Assert.True(HvRules.Independent(Guid.NewGuid(),a,a));}
    [Fact] public void ExternalReportNeedsActualReference(){Assert.False(HvRules.ExternalComplete("Required",null));Assert.False(HvRules.ExternalComplete("SentManually",""));Assert.True(HvRules.ExternalComplete("SentManually","REF"));}
    [Fact] public void NoReportWithoutStartedTransfer(){Assert.False(HvRules.CanReport(new BloodTransfer()));Assert.True(HvRules.CanReport(new BloodTransfer{StartedAt=DateTime.UtcNow}));}
}
