using Application.Common.Models;
using Application.Core.PatientAdmission_core;
using Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Application.Core.PatientAdmission_core.Commands.UpdateAdmission;
using Application.Core.PatientAdmission_core.Commands.UpdatePatientIdentity;
using Application.Core.PatientAdmission_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web.Endpoints;

public class PatientAdmission : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).RequireAuthorization()
            .MapPost(BindPatientAdmissionPaging,"BindPaging")
            .MapPost(CreatePatientAdmission)
            .MapPut(UpdatePatientAdmission)
            .MapPut(UpdatePatientIdentity,"PatientIdentity")
            .MapGet(FindPatientAdmission,"{guid:guid}")
            .MapGet(FindPatientByIdentity,"FindPatient")
            .MapGet(GetPatientAdmissionHistory,"{guid:guid}/History")
            .MapGet(GetPatientAdmissionReviews,"{guid:guid}/Reviews")
            .MapPost(GetPatientAdmissionStats,"Stats")
            .MapPost(ReviewPatientAdmission,"Review");
    }
    public async Task<Ok<PaginatedList<AdmissionView>>> BindPatientAdmissionPaging(ISender sender,PatientAdmissionWithPaginationQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Created<Guid>> CreatePatientAdmission(ISender sender,CreatePatientAdmissionCommand command,CancellationToken ct){var id=await sender.Send(command,ct);return TypedResults.Created($"/Identity/PatientAdmission/{id}",id);}
    public async Task<NoContent> UpdatePatientAdmission(ISender sender,UpdateAdmissionCommand command,CancellationToken ct){await sender.Send(command,ct);return TypedResults.NoContent();}
    public async Task<NoContent> UpdatePatientIdentity(ISender sender,UpdatePatientIdentityCommand command,CancellationToken ct){await sender.Send(command,ct);return TypedResults.NoContent();}
    public async Task<Ok<AdmissionView>> FindPatientAdmission(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new PatientAdmissionFindByIdQuery(guid),ct));
    public async Task<Ok<PatientIdentityView?>> FindPatientByIdentity(ISender sender,string identificationType,string identificationNumber,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new PatientFindByIdentityQuery(identificationType,identificationNumber),ct));
    public async Task<Ok<List<AdmissionView>>> GetPatientAdmissionHistory(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new PatientAdmissionHistoryQuery(guid),ct));
    public async Task<Ok<List<AdmissionReviewView>>> GetPatientAdmissionReviews(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new PatientAdmissionReviewsQuery(guid),ct));
    public async Task<Ok<AdmissionStats>> GetPatientAdmissionStats(ISender sender,PatientAdmissionStatsQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<NoContent> ReviewPatientAdmission(ISender sender,ReviewAdmissionCommand command,CancellationToken ct){await sender.Send(command,ct);return TypedResults.NoContent();}
}
