using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
using Application.Core.Quality_core;
using Application.Core.Quality_core.Commands;
using Application.Core.Quality_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Web.Endpoints;
public class HemoQuality : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group=app.MapGroup(this).RequireAuthorization()
            .AddEndpointFilter(async (context,next) =>
            {
                var http=context.HttpContext;
                var auth=http.RequestServices.GetRequiredService<AuthService>();
                var service=http.RequestServices.GetRequiredService<UserService>();
                var key=http.User.FindFirst("TokenKey")?.Value;
                if(auth.GetTokenExpireDate() is not DateTime expires || expires<=DateTime.Now ||
                   !Guid.TryParse(key,out _) || !service.CheckToken(key!))
                    return Results.Unauthorized();
                return await next(context);
            });
        group.MapGet("Capabilities", Capabilities).WithName("HEMO.HemoQuality.Capabilities");
        group.MapPost("EquipmentList", EquipmentList).WithName("HEMO.HemoQuality.EquipmentList");
        group.MapGet("EquipmentDetail/{guid:guid}", EquipmentDetail).WithName("HEMO.HemoQuality.EquipmentDetail");
        group.MapPost("EquipmentStats", EquipmentStats).WithName("HEMO.HemoQuality.EquipmentStats");
        group.MapPost("Incidents", Incidents).WithName("HEMO.HemoQuality.Incidents");
        group.MapPost("EquipmentCreate", EquipmentCreate).WithName("HEMO.HemoQuality.EquipmentCreate");
        group.MapPost("EquipmentEdit", EquipmentEdit).WithName("HEMO.HemoQuality.EquipmentEdit");
        group.MapPost("EquipmentService", EquipmentService).WithName("HEMO.HemoQuality.EquipmentService");
        group.MapPost("EquipmentHold", EquipmentHold).WithName("HEMO.HemoQuality.EquipmentHold");
        group.MapPost("EquipmentRelease", EquipmentRelease).WithName("HEMO.HemoQuality.EquipmentRelease");
        group.MapPost("EquipmentArchive", EquipmentArchive).WithName("HEMO.HemoQuality.EquipmentArchive");
        group.MapPost("IncidentReport", IncidentReport).WithName("HEMO.HemoQuality.IncidentReport");
        group.MapPost("IncidentResolve", IncidentResolve).WithName("HEMO.HemoQuality.IncidentResolve");
        group.MapPost("EquipmentTemperature", EquipmentTemperature).WithName("HEMO.HemoQuality.EquipmentTemperature");
        group.MapGet("Facility", Facility).WithName("HEMO.HemoQuality.Facility");
        group.MapPost("FacilitySave", FacilitySave).WithName("HEMO.HemoQuality.FacilitySave");
        group.MapPost("Stats", Stats).WithName("HEMO.HemoQuality.Stats");
        group.MapPost("QcList", QcList).WithName("HEMO.HemoQuality.QcList");
        group.MapGet("QcDetail/{guid:guid}", QcDetail).WithName("HEMO.HemoQuality.QcDetail");
        group.MapPost("QcCreate", QcCreate).WithName("HEMO.HemoQuality.QcCreate");
        group.MapPost("QcEdit", QcEdit).WithName("HEMO.HemoQuality.QcEdit");
        group.MapPost("QcLine", QcLine).WithName("HEMO.HemoQuality.QcLine");
        group.MapPost("QcSubmit", QcSubmit).WithName("HEMO.HemoQuality.QcSubmit");
        group.MapPost("QcApprove", QcApprove).WithName("HEMO.HemoQuality.QcApprove");
        group.MapPost("QcReject", QcReject).WithName("HEMO.HemoQuality.QcReject");
        group.MapPost("CommitteeList", CommitteeList).WithName("HEMO.HemoQuality.CommitteeList");
        group.MapGet("CommitteeDetail/{guid:guid}", CommitteeDetail).WithName("HEMO.HemoQuality.CommitteeDetail");
        group.MapPost("CommitteeCreate", CommitteeCreate).WithName("HEMO.HemoQuality.CommitteeCreate");
        group.MapPost("CommitteeEdit", CommitteeEdit).WithName("HEMO.HemoQuality.CommitteeEdit");
        group.MapPost("CommitteeLine", CommitteeLine).WithName("HEMO.HemoQuality.CommitteeLine");
        group.MapPost("CommitteeSubmit", CommitteeSubmit).WithName("HEMO.HemoQuality.CommitteeSubmit");
        group.MapPost("CommitteeApprove", CommitteeApprove).WithName("HEMO.HemoQuality.CommitteeApprove");
        group.MapPost("CommitteeReject", CommitteeReject).WithName("HEMO.HemoQuality.CommitteeReject");
        group.MapPost("CommitteeClose", CommitteeClose).WithName("HEMO.HemoQuality.CommitteeClose");
        group.MapGet("CommitteeOwners", CommitteeOwners).WithName("HEMO.HemoQuality.CommitteeOwners");
        group.MapPost("AuditList", AuditList).WithName("HEMO.HemoQuality.AuditList");
        group.MapGet("AuditDetail/{guid:guid}", AuditDetail).WithName("HEMO.HemoQuality.AuditDetail");
        group.MapPost("AuditCreate", AuditCreate).WithName("HEMO.HemoQuality.AuditCreate");
        group.MapPost("AuditEdit", AuditEdit).WithName("HEMO.HemoQuality.AuditEdit");
        group.MapPost("AuditLine", AuditLine).WithName("HEMO.HemoQuality.AuditLine");
        group.MapPost("AuditSubmit", AuditSubmit).WithName("HEMO.HemoQuality.AuditSubmit");
        group.MapPost("AuditApprove", AuditApprove).WithName("HEMO.HemoQuality.AuditApprove");
        group.MapPost("AuditReject", AuditReject).WithName("HEMO.HemoQuality.AuditReject");
        group.MapPost("TrainingList", TrainingList).WithName("HEMO.HemoQuality.TrainingList");
        group.MapGet("TrainingDetail/{guid:guid}", TrainingDetail).WithName("HEMO.HemoQuality.TrainingDetail");
        group.MapPost("TrainingCreate", TrainingCreate).WithName("HEMO.HemoQuality.TrainingCreate");
        group.MapPost("TrainingEdit", TrainingEdit).WithName("HEMO.HemoQuality.TrainingEdit");
        group.MapPost("TrainingLine", TrainingLine).WithName("HEMO.HemoQuality.TrainingLine");
        group.MapPost("TrainingSubmit", TrainingSubmit).WithName("HEMO.HemoQuality.TrainingSubmit");
        group.MapPost("TrainingApprove", TrainingApprove).WithName("HEMO.HemoQuality.TrainingApprove");
        group.MapPost("TrainingReject", TrainingReject).WithName("HEMO.HemoQuality.TrainingReject");
        group.MapGet("TrainingOwners", TrainingOwners).WithName("HEMO.HemoQuality.TrainingOwners");
        group.MapPost("CommitteeProgress", CommitteeProgress).WithName("HEMO.HemoQuality.CommitteeProgress");
        group.MapPost("CommitteeVerify", CommitteeVerify).WithName("HEMO.HemoQuality.CommitteeVerify");
        group.MapPost("CommitteeReturn", CommitteeReturn).WithName("HEMO.HemoQuality.CommitteeReturn");
    }
    public async Task<Ok<QualityCapabilities>> Capabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityCapabilitiesQuery(),ct));
    public async Task<Ok<PaginatedList<EquipmentRow>>> EquipmentList(ISender sender,EquipmentFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new EquipmentListQuery(input),ct));
    public async Task<Ok<EquipmentDetail>> EquipmentDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new EquipmentDetailQuery(guid),ct));
    public async Task<Ok<EquipmentSummary>> EquipmentStats(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new EquipmentSummaryQuery(),ct));
    public async Task<Ok<PaginatedList<IncidentRow>>> Incidents(ISender sender,EquipmentIncidentsQuery input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(input,ct));
    public async Task<Ok<Guid>> EquipmentCreate(ISender sender,EquipmentInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new CreateEquipmentCommand(input),ct));
    public async Task<NoContent> EquipmentEdit(ISender sender,EquipmentEdit input,CancellationToken ct){await sender.Send(new EditEquipmentCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> EquipmentService(ISender sender,ServiceInput input,CancellationToken ct){await sender.Send(new EquipmentServiceCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> EquipmentHold(ISender sender,EquipmentActionInput input,CancellationToken ct){await sender.Send(new EquipmentStateCommand(input,"Hold"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> EquipmentRelease(ISender sender,EquipmentActionInput input,CancellationToken ct){await sender.Send(new EquipmentStateCommand(input,"Release"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> EquipmentArchive(ISender sender,EquipmentActionInput input,CancellationToken ct){await sender.Send(new EquipmentStateCommand(input,"Archive"),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> IncidentReport(ISender sender,IncidentInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new ReportEquipmentIncidentCommand(input),ct));
    public async Task<NoContent> IncidentResolve(ISender sender,IncidentResolve input,CancellationToken ct){await sender.Send(new ResolveEquipmentIncidentCommand(input),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> EquipmentTemperature(ISender sender,EquipmentTemperatureInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new EquipmentTemperatureCommand(input),ct));
    public async Task<Ok<FacilityView>> Facility(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new FacilityQuery(),ct));
    public async Task<NoContent> FacilitySave(ISender sender,FacilityInput input,CancellationToken ct){await sender.Send(new SaveFacilityCommand(input),ct);return TypedResults.NoContent();}
    public async Task<Ok<QualityStats>> Stats(ISender sender,QualityStatsFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityStatsQuery(input),ct));
    public async Task<Ok<PaginatedList<QualityRow>>> QcList(ISender sender,QualityFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityListQuery("Qc",input),ct));
    public async Task<Ok<QualityDetail>> QcDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityDetailQuery("Qc",guid),ct));
    public async Task<Ok<Guid>> QcCreate(ISender sender,QualityCreate input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new CreateQualityCommand("Qc",input),ct));
    public async Task<NoContent> QcEdit(ISender sender,QualityEdit input,CancellationToken ct){await sender.Send(new EditQualityCommand("Qc",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> QcLine(ISender sender,QualityLineInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new SaveQualityLineCommand("Qc",input),ct));
    public async Task<NoContent> QcSubmit(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Qc","Submit",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> QcApprove(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Qc","Approve",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> QcReject(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Qc","Reject",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<PaginatedList<QualityRow>>> CommitteeList(ISender sender,QualityFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityListQuery("Committee",input),ct));
    public async Task<Ok<QualityDetail>> CommitteeDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityDetailQuery("Committee",guid),ct));
    public async Task<Ok<Guid>> CommitteeCreate(ISender sender,QualityCreate input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new CreateQualityCommand("Committee",input),ct));
    public async Task<NoContent> CommitteeEdit(ISender sender,QualityEdit input,CancellationToken ct){await sender.Send(new EditQualityCommand("Committee",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> CommitteeLine(ISender sender,QualityLineInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new SaveQualityLineCommand("Committee",input),ct));
    public async Task<NoContent> CommitteeSubmit(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Committee","Submit",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CommitteeApprove(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Committee","Approve",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CommitteeReject(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Committee","Reject",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CommitteeClose(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Committee","Close",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<List<QualityOwner>>> CommitteeOwners(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityOwnersQuery("Committee"),ct));
    public async Task<Ok<PaginatedList<QualityRow>>> AuditList(ISender sender,QualityFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityListQuery("Audit",input),ct));
    public async Task<Ok<QualityDetail>> AuditDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityDetailQuery("Audit",guid),ct));
    public async Task<Ok<Guid>> AuditCreate(ISender sender,QualityCreate input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new CreateQualityCommand("Audit",input),ct));
    public async Task<NoContent> AuditEdit(ISender sender,QualityEdit input,CancellationToken ct){await sender.Send(new EditQualityCommand("Audit",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> AuditLine(ISender sender,QualityLineInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new SaveQualityLineCommand("Audit",input),ct));
    public async Task<NoContent> AuditSubmit(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Audit","Submit",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> AuditApprove(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Audit","Approve",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> AuditReject(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Audit","Reject",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<PaginatedList<QualityRow>>> TrainingList(ISender sender,QualityFilter input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityListQuery("Training",input),ct));
    public async Task<Ok<QualityDetail>> TrainingDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityDetailQuery("Training",guid),ct));
    public async Task<Ok<Guid>> TrainingCreate(ISender sender,QualityCreate input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new CreateQualityCommand("Training",input),ct));
    public async Task<NoContent> TrainingEdit(ISender sender,QualityEdit input,CancellationToken ct){await sender.Send(new EditQualityCommand("Training",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<Guid>> TrainingLine(ISender sender,QualityLineInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new SaveQualityLineCommand("Training",input),ct));
    public async Task<NoContent> TrainingSubmit(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Training","Submit",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> TrainingApprove(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Training","Approve",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> TrainingReject(ISender sender,QualityAction input,CancellationToken ct){await sender.Send(new QualityReviewCommand("Training","Reject",input),ct);return TypedResults.NoContent();}
    public async Task<Ok<List<QualityOwner>>> TrainingOwners(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new QualityOwnersQuery("Training"),ct));
    public async Task<NoContent> CommitteeProgress(ISender sender,ResolutionProgress input,CancellationToken ct){await sender.Send(new ResolutionCommand("Progress",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CommitteeVerify(ISender sender,ResolutionProgress input,CancellationToken ct){await sender.Send(new ResolutionCommand("Verify",input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CommitteeReturn(ISender sender,ResolutionProgress input,CancellationToken ct){await sender.Send(new ResolutionCommand("Return",input),ct);return TypedResults.NoContent();}
}
