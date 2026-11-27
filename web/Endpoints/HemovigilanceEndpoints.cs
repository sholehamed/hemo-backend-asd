using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
using Application.Core.Hemovigilance_core;
using Application.Core.Hemovigilance_core.Commands;
using Application.Core.Hemovigilance_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Web.Endpoints;
public class Hemovigilance : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).RequireAuthorization()
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
            })
            .MapGet(Capabilities,"Capabilities").MapPost(List,"List").MapGet(Detail,"Detail/{guid:guid}").MapPost(Stats,"Stats").MapPost(Candidates,"Candidates").MapGet(Owners,"Owners")
            .MapPost(Report,"Report").MapPost(AssignCapa,"AssignCapa").MapPost(Reports,"Reports").MapGet(ReportDetail,"ReportDetail/{guid:guid}").MapPost(Archive,"Archive").MapPost(ReportExternal,"ReportExternal")
            .MapPost(Observe,"Observe").MapPost(DoctorReview,"DoctorReview").MapPost(BankReview,"BankReview").MapPost(Investigate,"Investigate").MapPost(ReturnToDoctor,"ReturnToDoctor").MapPost(PublishRca,"PublishRca").MapPost(Close,"Close").MapPost(External,"External").MapPost(CapaProgress,"CapaProgress").MapPost(CapaVerify,"CapaVerify").MapPost(CapaReject,"CapaReject");
    }
    public async Task<Ok<HvCapabilities>> Capabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvCapabilitiesQuery(),ct));
    public async Task<Ok<PaginatedList<HvCaseRow>>> List(ISender sender,HvFilters filters,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvListQuery(filters),ct));
    public async Task<Ok<HvDetail>> Detail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvDetailQuery(guid),ct));
    public async Task<Ok<HvSummary>> Stats(ISender sender,HvFilters filters,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvStatsQuery(filters),ct));
    public async Task<Ok<PaginatedList<HvCandidate>>> Candidates(ISender sender,HvCandidatesQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<List<HvOwner>>> Owners(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvOwnersQuery(),ct));
    public async Task<Created<Guid>> Report(ISender sender,ReactionInput input,CancellationToken ct){var id=await sender.Send(new ReportReactionCommand(input),ct);return TypedResults.Created($"/Identity/Hemovigilance/Detail/{id}",id);}
    public async Task<Created<Guid>> AssignCapa(ISender sender,CapaCreateInput input,CancellationToken ct){var id=await sender.Send(new AssignCapaCommand(input),ct);return TypedResults.Created($"/Identity/Hemovigilance/Capa/{id}",id);}
    public async Task<Ok<PaginatedList<HvReportRow>>> Reports(ISender sender,HvReportsQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<HvReportDetail>> ReportDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new HvReportDetailQuery(guid),ct));
    public async Task<Created<Guid>> Archive(ISender sender,ReportInput input,CancellationToken ct){var id=await sender.Send(new ArchiveHvReportCommand(input),ct);return TypedResults.Created($"/Identity/Hemovigilance/ReportDetail/{id}",id);}
    public async Task<NoContent> ReportExternal(ISender sender,ReportExternalInput input,CancellationToken ct){await sender.Send(new RecordReportExternalCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Observe(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"Observe"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> DoctorReview(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"DoctorReview"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> BankReview(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"BankReview"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Investigate(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"Investigate"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> ReturnToDoctor(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"ReturnToDoctor"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> PublishRca(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"PublishRca"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Close(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"Close"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> External(ISender sender,ReviewInput input,CancellationToken ct){await sender.Send(new ReviewReactionCommand(input,"External"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CapaProgress(ISender sender,CapaActionInput input,CancellationToken ct){await sender.Send(new CapaActionCommand(input,"CapaProgress"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CapaVerify(ISender sender,CapaActionInput input,CancellationToken ct){await sender.Send(new CapaActionCommand(input,"CapaVerify"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> CapaReject(ISender sender,CapaActionInput input,CancellationToken ct){await sender.Send(new CapaActionCommand(input,"CapaReject"),ct);return TypedResults.NoContent();}
}
