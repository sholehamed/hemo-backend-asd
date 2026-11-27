using Application.Core.BloodBank_core;
using Domain.Entities;
using Xunit;
namespace Hemo.Admission.Tests;
public class BankRulesTests
{
    [Theory]
    [InlineData("ReceivedHold","Inspect",false,false,"Available")]
    [InlineData("Available","Reserve",false,false,"Reserved")]
    [InlineData("Reserved","Ready",true,false,null)]
    [InlineData("Reserved","StartThaw",true,false,"Thawing")]
    [InlineData("Thawing","FinishThaw",true,false,"Reserved")]
    [InlineData("Reserved","Ready",true,true,"Ready")]
    [InlineData("Reserved","Ready",false,false,"Ready")]
    [InlineData("Ready","Unreserve",true,true,"Quarantined")]
    [InlineData("Available","Discard",false,false,null)]
    [InlineData("Quarantined","Discard",false,false,"Discarded")]
    [InlineData("Discarded","Inspect",false,false,null)]
    public void StateTransitions(string state,string action,bool thaw,bool complete,string? next)=>Assert.Equal(next,BankRules.Next(state,action,thaw,complete));
    [Fact] public void ExpiryBoundaryIsExclusive()=>Assert.True(BankRules.IsExpired(DateTime.UnixEpoch,DateTime.UnixEpoch));
    [Fact] public void NoAutomaticCompatibilityFromGroup()
    {
        var test=new BankCompatibilityTest{Conclusion="Accepted",SecondSample="Confirmed",ImmediateSpin="NonReactive",Phase37="NonReactive",Ahg="Reactive",Dat="NonReactive",AntibodyScreen="NonReactive"};
        Assert.False(BankRules.AcceptedLab(test));test.Ahg="NonReactive";Assert.True(BankRules.AcceptedLab(test));
        test.Dat="Reactive";Assert.False(BankRules.AcceptedLab(test));test.Notes="Documented laboratory review";Assert.True(BankRules.AcceptedLab(test));
    }
}
