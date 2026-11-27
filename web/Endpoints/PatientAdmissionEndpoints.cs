using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
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
        group.MapGet("Capabilities", GetCapabilities).WithName("HEMO.PatientAdmission.GetCapabilities");
        group.MapPost("BindPaging", BindPatientAdmissionPaging).WithName("HEMO.PatientAdmission.BindPatientAdmissionPaging");
        group.MapPost("", CreatePatientAdmission).WithName("HEMO.PatientAdmission.CreatePatientAdmission");
        group.MapPut("", UpdatePatientAdmission).WithName("HEMO.PatientAdmission.UpdatePatientAdmission");
        group.MapPut("PatientIdentity", UpdatePatientIdentity).WithName("HEMO.PatientAdmission.UpdatePatientIdentity");
        group.MapGet("{guid:guid}", FindPatientAdmission).WithName("HEMO.PatientAdmission.FindPatientAdmission");
        group.MapGet("FindPatient", FindPatientByIdentity).WithName("HEMO.PatientAdmission.FindPatientByIdentity");
        group.MapGet("{guid:guid}/History", GetPatientAdmissionHistory).WithName("HEMO.PatientAdmission.GetPatientAdmissionHistory");
        group.MapGet("{guid:guid}/Reviews", GetPatientAdmissionReviews).WithName("HEMO.PatientAdmission.GetPatientAdmissionReviews");
        group.MapPost("Stats", GetPatientAdmissionStats).WithName("HEMO.PatientAdmission.GetPatientAdmissionStats");
        group.MapPost("Review", ReviewPatientAdmission).WithName("HEMO.PatientAdmission.ReviewPatientAdmission");
    }
    public async Task<Ok<AdmissionCapabilities>> GetCapabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new AdmissionCapabilitiesQuery(),ct));
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
