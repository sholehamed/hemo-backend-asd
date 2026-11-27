using Application.Core.BloodRequest_core;
using Domain.Entities;
using Xunit;
namespace Hemo.Admission.Tests;

public class BloodRequestWorkflowTests
{
    [Theory]
    [InlineData("Draft","Submit","AwaitingDoctor")]
    [InlineData("Returned","Submit","AwaitingDoctor")]
    [InlineData("AwaitingDoctor","Approve","AwaitingBank")]
    [InlineData("AwaitingDoctor","Return","Returned")]
    [InlineData("AwaitingDoctor","Reject","Rejected")]
    [InlineData("Draft","Cancel","Cancelled")]
    [InlineData("AwaitingDoctor","Cancel","Cancelled")]
    [InlineData("AwaitingBank","Cancel",null)]
    [InlineData("Rejected","Submit",null)]
    [InlineData("Cancelled","Approve",null)]
    [InlineData("Draft","Approve",null)]
    [InlineData("AwaitingDoctor","Submit",null)]
    public void ValidatesTransitions(string state,string action,string? next)=>Assert.Equal(next,BloodRequestWorkflow.Next(state,action));
    [Fact]
    public void ValidatesProductAndQuantityWithoutInferringClinicalIndication()
    {
        Assert.False(BloodRequestWorkflow.HasValidItems(Array.Empty<BloodRequestItem>()));
        Assert.True(BloodRequestWorkflow.HasValidItems(new[]{new BloodRequestItem{ProductCode="RBC",Quantity=1}}));
        Assert.False(BloodRequestWorkflow.HasValidItems(new[]{new BloodRequestItem{ProductCode="RBC",Quantity=0}}));
        Assert.False(BloodRequestWorkflow.HasValidItems(new[]{new BloodRequestItem{ProductCode="Invalid",Quantity=1}}));
        Assert.False(BloodRequestWorkflow.HasValidItems(new[]{new BloodRequestItem{ProductCode="RBC",Quantity=1},new BloodRequestItem{ProductCode="RBC",Quantity=1}}));
    }
    [Theory]
    [InlineData("Return",true)]
    [InlineData("Reject",true)]
    [InlineData("Cancel",true)]
    [InlineData("Approve",false)]
    public void RequiresReasonForNegativeActions(string action,bool required)=>Assert.Equal(required,BloodRequestWorkflow.RequiresReason(action));
}
