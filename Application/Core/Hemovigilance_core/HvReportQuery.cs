namespace Application.Core.Hemovigilance_core;
internal static class HvReportQuery
{
    public static IQueryable<HemoReaction> Scope(IApplicationDbContext db,Guid org)=>db.HemoReaction.AsNoTracking().Where(x=>x.OrgLocationGUID==org&&x.Transfer.OrgLocationGUID==org&&x.Transfer.Admission.OrgLocationGUID==org&&x.Transfer.Unit.OrgLocationGUID==org);
    public static IQueryable<HemoReaction> Filter(IQueryable<HemoReaction> q,HvFilters f)
    {
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(x=>x.CaseNumber.Contains(s)||x.Transfer.PatientName.Contains(s)||x.Transfer.HisNumber.Contains(s)||x.Transfer.UnitCode.Contains(s));}
        if(!string.IsNullOrWhiteSpace(f.Status))q=f.Status=="Open"?q.Where(x=>x.Status!="Closed"):q.Where(x=>x.Status==f.Status);
        if(!string.IsNullOrWhiteSpace(f.Ward))q=q.Where(x=>x.Transfer.Ward==f.Ward);
        if(!string.IsNullOrWhiteSpace(f.ProductCode))q=q.Where(x=>x.Transfer.ProductCode==f.ProductCode);
        if(!string.IsNullOrWhiteSpace(f.Severity))q=q.Where(x=>(x.Severity??x.InitialSeverity)==f.Severity);
        if(f.TransferGuid.HasValue)q=q.Where(x=>x.BloodTransferGUID==f.TransferGuid);
        if(f.From.HasValue)q=q.Where(x=>x.OnsetAt>=f.From.Value.UtcDateTime);
        if(f.To.HasValue)q=q.Where(x=>x.OnsetAt<=f.To.Value.UtcDateTime);
        return q;
    }
    public static async Task<HvSummary> Summary(IApplicationDbContext db,Guid org,HvFilters f,CancellationToken ct)
    {
        var q=Filter(Scope(db,org),f);
        var rows=await q.Select(x=>new{x.Status,x.DiagnosisCode,Severity=x.Severity??x.InitialSeverity,x.Transfer.Ward,x.Transfer.ProductCode,x.ExternalStatus}).ToListAsync(ct);
        var capa=db.HemoCapa.AsNoTracking().Where(x=>q.Any(r=>r.GUID==x.HemoReactionGUID)&&x.Status!="Verified");
        var now=DateTime.UtcNow;
        var started=db.BloodTransfer.AsNoTracking().Where(x=>x.OrgLocationGUID==org&&x.Admission.OrgLocationGUID==org&&x.Unit.OrgLocationGUID==org&&x.StartedAt.HasValue);
        if(f.From.HasValue)started=started.Where(x=>x.StartedAt>=f.From.Value.UtcDateTime);
        if(f.To.HasValue)started=started.Where(x=>x.StartedAt<=f.To.Value.UtcDateTime);
        if(!string.IsNullOrWhiteSpace(f.Ward))started=started.Where(x=>x.Ward==f.Ward);
        if(!string.IsNullOrWhiteSpace(f.ProductCode))started=started.Where(x=>x.ProductCode==f.ProductCode);
        List<HvCount> Group(IEnumerable<string> keys)=>keys.GroupBy(x=>x).OrderByDescending(g=>g.Count()).ThenBy(g=>g.Key).Select(g=>new HvCount(g.Key,g.Count())).ToList();
        int N(string s)=>rows.Count(x=>x.Status==s);
        // Independent counts, not a denominator for an incidence rate (onset and start cohorts differ).
        return new(rows.Count,rows.Count-N("Closed"),N("Closed"),N("Reported"),N("DoctorReviewed"),N("RcaOpen"),N("CapaMonitoring"),await capa.CountAsync(ct),await capa.CountAsync(x=>x.DueAt<now,ct),rows.Count(x=>x.ExternalStatus=="Required"),await started.CountAsync(ct),
            Group(rows.Select(x=>x.DiagnosisCode??"Undetermined")),Group(rows.Select(x=>x.Severity)),Group(rows.Select(x=>x.Ward)),Group(rows.Select(x=>x.ProductCode)),Group(rows.Select(x=>x.Status)));
    }
}
