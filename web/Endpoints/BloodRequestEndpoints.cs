using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodRequest_core.Commands;
using Application.Core.BloodRequest_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Web.Endpoints;

public class BloodRequest : EndpointGroupBase
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
            .MapGet(Capabilities,"Capabilities")
            .MapPost(List,"BindPaging").MapPost(Stats,"Stats")
            .MapGet(Detail,"{guid:guid}")
            .MapPost(BankQueue,"BankQueue").MapGet(BankDetail,"Bank/{guid:guid}")
            .MapPost(CreateNurse,"Nurse").MapPost(CreateDoctor,"Doctor")
            .MapPut(Update).MapPost(Submit,"Submit").MapPost(Cancel,"Cancel").MapPost(Decide,"Decide");
    }
    public async Task<Ok<RequestCapabilities>> Capabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new BloodRequestCapabilitiesQuery(),ct));
    public async Task<Ok<PaginatedList<RequestRow>>> List(ISender sender,BloodRequestListQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<RequestStats>> Stats(ISender sender,BloodRequestStatsQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<RequestDetail>> Detail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new BloodRequestDetailQuery(guid),ct));
    public async Task<Ok<PaginatedList<RequestRow>>> BankQueue(ISender sender,BloodRequestBankQueueQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<RequestDetail>> BankDetail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new BloodRequestBankDetailQuery(guid),ct));
    public async Task<Created<Guid>> CreateNurse(ISender sender,RequestInput input,CancellationToken ct){var id=await sender.Send(new CreateNurseRequestCommand(input),ct);return TypedResults.Created($"/Identity/BloodRequest/{id}",id);}
    public async Task<Created<Guid>> CreateDoctor(ISender sender,RequestInput input,CancellationToken ct){var id=await sender.Send(new CreateDoctorRequestCommand(input),ct);return TypedResults.Created($"/Identity/BloodRequest/{id}",id);}
    public async Task<NoContent> Update(ISender sender,UpdateBloodRequestCommand command,CancellationToken ct){await sender.Send(command,ct);return TypedResults.NoContent();}
    public async Task<NoContent> Submit(ISender sender,DecisionInput input,CancellationToken ct){await sender.Send(new SubmitBloodRequestCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Cancel(ISender sender,DecisionInput input,CancellationToken ct){await sender.Send(new CancelBloodRequestCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Decide(ISender sender,DecisionInput input,CancellationToken ct){await sender.Send(new DecideBloodRequestCommand(input),ct);return TypedResults.NoContent();}
}
