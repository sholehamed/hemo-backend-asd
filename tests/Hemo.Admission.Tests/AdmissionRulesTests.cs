using Application.Core.PatientAdmission_core;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Xunit;
namespace Hemo.Admission.Tests;
public class AdmissionRulesTests
{
    [Theory]
    [InlineData("1234567891",true)]
    [InlineData("۱۲۳۴۵۶۷۸۹۱",true)]
    [InlineData("1234567890",false)]
    [InlineData("1111111111",false)]
    [InlineData("",false)]
    public void NationalIdChecksum(string input,bool valid)=>Assert.Equal(valid,AdmissionRules.IsNationalId(input));

    [Fact]
    public void NormalizesPersianAndArabicDigits()=>Assert.Equal("123AB",AdmissionRules.NormalizeIdentity(" ۱٢۳ ab "));

    [Theory]
    [InlineData(31,true)]
    [InlineData(0,false)]
    [InlineData(30,false)]
    [InlineData(63,false)]
    public void RequiresExactChecklist(int mask,bool expected)=>Assert.Equal(expected,AdmissionRules.CompleteChecklist(mask));

    [Theory]
    [InlineData("Identity","Verified",31,null,true)]
    [InlineData("Identity","Verified",30,null,false)]
    [InlineData("Identity","Mismatch",0,null,false)]
    [InlineData("Identity","Mismatch",0,"test",true)]
    [InlineData("History","Reviewed",0,"test",true)]
    [InlineData("History","Reviewed",0,null,false)]
    [InlineData("Clearance","Draft",0,null,true)]
    [InlineData("Clearance","Ready",31,null,true)]
    [InlineData("Clearance","Ready",0,null,false)]
    [InlineData("Other","Ready",31,null,false)]
    public void ReviewValidation(string kind,string outcome,int checklist,string? notes,bool expected)
    {
        var cmd=new ReviewAdmissionCommand(Guid.NewGuid(),kind,outcome,checklist,notes,new byte[]{1});
        Assert.Equal(expected,new ReviewAdmissionValidator().Validate(cmd).IsValid);
    }
}
