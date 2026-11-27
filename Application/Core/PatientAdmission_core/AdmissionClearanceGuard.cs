using Application.Common.Exceptions;
namespace Application.Core.PatientAdmission_core;

// Administrative gate only; never infer clinical readiness or discharge from these records.
internal static class AdmissionClearanceGuard
{
    public static async Task RequireNoActiveBloodFlow(IApplicationDbContext db,Guid admission,Guid org,CancellationToken ct)
    {
        if(await db.BloodRequest.AnyAsync(r=>r.PatientAdmissionGUID==admission&&r.PatientAdmission.OrgLocationGUID==org&&
            (r.Status=="Draft"||r.Status=="Returned"||r.Status=="AwaitingDoctor"),ct))
            throw new ServiceExeption("درخواست خون در انتظار تکمیل یا تصمیم پزشک است؛ ابتدا گردش درخواست را تعیین تکلیف کنید.");
        if(await db.BankBloodUnit.AnyAsync(u=>u.OrgLocationGUID==org&&u.BloodRequest!=null&&u.BloodRequest.PatientAdmissionGUID==admission&&
            (u.Status=="Reserved"||u.Status=="Ready"||u.Status=="Thawing"||u.Status=="Quarantined"||u.Status=="ReceivedHold"||u.Status=="Available"),ct))
            throw new ServiceExeption("واحد خون دارای گردش باز در بانک است؛ ابتدا وضعیت واحد را تعیین تکلیف کنید.");
        if(await db.BloodTransfer.AnyAsync(t=>t.OrgLocationGUID==org&&t.PatientAdmissionGUID==admission&&
            t.Status!="Completed"&&t.Status!="Returned"&&t.Status!="Disposed",ct))
            throw new ServiceExeption("حواله یا گردش تزریق هنوز باز است؛ ابتدا وضعیت حواله را تعیین تکلیف کنید.");
        // AwaitingBank is also retained by completed orders in ST03–07.
        // Never mistake that status alone for an outstanding order. Require completed quantities;
        // partial/cancelled remainder requires a documented future disposition workflow.
        if(await db.BloodRequestItem.AnyAsync(i=>i.BloodRequest.PatientAdmissionGUID==admission&&
            i.BloodRequest.PatientAdmission.OrgLocationGUID==org&&i.BloodRequest.Status=="AwaitingBank"&&
            db.BloodTransfer.Count(t=>t.OrgLocationGUID==org&&t.BloodRequestGUID==i.BloodRequestGUID&&
                t.ProductCode==i.ProductCode&&t.Status=="Completed")<i.Quantity,ct))
            throw new ServiceExeption("درخواست تأییدشده دارای تعداد فرآورده تعیین‌تکلیف‌نشده است.");
    }
}
