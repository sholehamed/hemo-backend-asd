using Application.Core.BloodBank_core;
namespace Application.Core.Quality_core;
public static class QualityValidation
{
    private static bool T(string? s,int max)=>QualityRules.Text(s,max);
    private static bool O(string? s,int max)=>QualityRules.Optional(s,max);
    public static bool Header(string kind,string title,string ward,DateTimeOffset date,QualityHeader? h)
    {
        if(!QualityRules.Kinds.Contains(kind)||!T(title,300)||!T(ward,120)||date==default||h==null||kind!="Training"&&date.UtcDateTime>DateTime.UtcNow)return false;
        if(!O(h.MaterialReference,1000)||!O(h.Nonconformance,4000)||!O(h.CorrectiveAction,4000))return false;
        return kind switch {
            "Qc"=>new[]{"Morning","Evening","Night","Other"}.Contains(h.Shift)&&T(h.Operator,200)&&(!h.WaterBathTemperature.HasValue||BankValidation.Temperature(h.WaterBathTemperature)),
            "Committee"=>T(h.Chair,200)&&T(h.Secretary,200)&&T(h.Participants,4000)&&T(h.Agenda,4000),
            "Audit"=>T(h.Checklist,200)&&T(h.Auditor,200),
            "Training"=>T(h.Instructor,200)&&h.DurationMinutes is >0 and <=10080&&h.Capacity is >0 and <=10000&&T(h.Audience,500),
            _=>false
        };
    }
    public static bool Line(string kind,string title,QualityLineData? d,Guid? owner,DateTimeOffset? due,DateTime occurred)
    {
        if(!T(title,300)||d==null||!O(d.Notes,4000))return false;
        return kind switch {
            "Qc"=>T(d.Lot,100)&&T(d.Manufacturer,200)&&d.Expiry.HasValue&&T(d.PositiveExpected,200)&&T(d.NegativeExpected,200)&&T(d.PositiveObserved,200)&&T(d.NegativeObserved,200)&&new[]{"Accepted","Rejected","Pending"}.Contains(d.Result)&&(d.Result!="Accepted"||d.Expiry.Value.UtcDateTime>occurred)&&(d.Result=="Accepted"||T(d.Notes,4000))&&owner==null&&due==null,
            "Committee"=>owner.HasValue&&owner!=Guid.Empty&&due.HasValue&&due.Value.UtcDateTime>occurred&&T(d.Indicator,1000),
            "Audit"=>T(d.Clause,200)&&d.Score is >=0 and <=2&&(d.Score==2||T(d.Notes,4000))&&owner==null&&due==null,
            "Training"=>owner.HasValue&&owner!=Guid.Empty&&due==null&&T(d.PersonnelCode,100)&&T(d.Ward,120)&&T(d.Shift,32)&&new[]{"Registered","Present","Absent"}.Contains(d.Attendance)&&(!d.PreScore.HasValue||QualityRules.ValidScore(d.PreScore))&&(!d.PostScore.HasValue||QualityRules.ValidScore(d.PostScore))&&(d.Attendance=="Present"||!d.PostScore.HasValue&&string.IsNullOrWhiteSpace(d.CertificateReference))&&O(d.CertificateReference,200),
            _=>false
        };
    }
    public static bool CanSubmit(string kind,QualityHeader h,IReadOnlyList<QualityLineData> rows,DateTime occurred)
    {
        if(rows.Count==0||occurred>DateTime.UtcNow)return false;
        return kind switch{
            "Qc"=>rows.All(x=>x.Result!="Pending")&&(!rows.Any(x=>x.Result=="Rejected")||T(h.Nonconformance,4000)&&T(h.CorrectiveAction,4000)),
            "Training"=>rows.Count<=h.Capacity&&rows.All(x=>x.Attendance!="Registered"),
            "Committee" or "Audit"=>true,_=>false
        };
    }
}
