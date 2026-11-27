using Application.Core.Transfusion_core;
using Domain.Entities;
using Xunit;
namespace Hemo.Admission.Tests;
public class TransferRulesTests
{
    [Theory]
    [InlineData("Dispatched","Receive","Received")]
    [InlineData("Dispatched","ReceiptHold","ReceiptHold")]
    [InlineData("Received","Start",null)]
    [InlineData("ReceiptHold","Verify",null)]
    [InlineData("Received","Verify","Verified")]
    [InlineData("Verified","Witness","Witnessed")]
    [InlineData("Witnessed","Start","InProgress")]
    [InlineData("InProgress","Stop","Stopped")]
    [InlineData("Stopped","Start",null)]
    [InlineData("InProgress","Complete","Completed")]
    [InlineData("Completed","Return",null)]
    [InlineData("Stopped","Return","Returning")]
    [InlineData("Returning","AcceptReturn","Returned")]
    [InlineData("Returned","Dispose","Disposed")]
    public void Transition(string status,string action,string? expected)=>Assert.Equal(expected,TransferRules.Next(status,action));
    [Fact] public void TwoDistinctUsersAreRequired(){var a=Guid.NewGuid();Assert.False(TransferRules.DistinctWitness(a,a));Assert.False(TransferRules.DistinctWitness(null,a));Assert.True(TransferRules.DistinctWitness(a,Guid.NewGuid()));}
    [Fact] public void ExactCodesNotInferred(){var t=new BloodTransfer{UnitCode="U-1",WristbandCode="W-1"};Assert.True(TransferRules.ExactCodes("U-1","W-1",t));Assert.False(TransferRules.ExactCodes("U-2","W-1",t));}
    [Theory]
    [InlineData("Received","T0",false)]
    [InlineData("Witnessed","T0",true)]
    [InlineData("InProgress","T15",true)]
    [InlineData("Completed","Mid",false)]
    [InlineData("Stopped","Post1h",true)]
    public void PhaseGate(string status,string phase,bool allowed)=>Assert.Equal(allowed,TransferRules.VitalAllowed(status,phase));
}
