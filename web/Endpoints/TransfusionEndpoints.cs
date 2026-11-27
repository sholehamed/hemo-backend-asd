using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
using Application.Core.Transfusion_core;
using Application.Core.Transfusion_core.Commands;
using Application.Core.Transfusion_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Web.Endpoints;
public class Transfusion : EndpointGroupBase
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
        group.MapGet("Capabilities", Capabilities).WithName("HEMO.Transfusion.Capabilities");
        group.MapPost("List", List).WithName("HEMO.Transfusion.List");
        group.MapGet("Detail/{guid:guid}", Detail).WithName("HEMO.Transfusion.Detail");
        group.MapPost("Stats", Stats).WithName("HEMO.Transfusion.Stats");
        group.MapPost("Candidates", Candidates).WithName("HEMO.Transfusion.Candidates");
        group.MapPost("Dispatch", Dispatch).WithName("HEMO.Transfusion.Dispatch");
        group.MapPost("Vital", Vital).WithName("HEMO.Transfusion.Vital");
        group.MapPost("Receive", Receive).WithName("HEMO.Transfusion.Receive");
        group.MapPost("ReceiptHold", ReceiptHold).WithName("HEMO.Transfusion.ReceiptHold");
        group.MapPost("Verify", Verify).WithName("HEMO.Transfusion.Verify");
        group.MapPost("Witness", Witness).WithName("HEMO.Transfusion.Witness");
        group.MapPost("Start", Start).WithName("HEMO.Transfusion.Start");
        group.MapPost("Complete", Complete).WithName("HEMO.Transfusion.Complete");
        group.MapPost("Stop", Stop).WithName("HEMO.Transfusion.Stop");
        group.MapPost("Return", Return).WithName("HEMO.Transfusion.Return");
        group.MapPost("AcceptReturn", AcceptReturn).WithName("HEMO.Transfusion.AcceptReturn");
        group.MapPost("Dispose", Dispose).WithName("HEMO.Transfusion.Dispose");
        group.MapPost("DoctorNote", DoctorNote).WithName("HEMO.Transfusion.DoctorNote");
    }
    public async Task<Ok<TransferCapabilities>> Capabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new TransferCapabilitiesQuery(),ct));
    public async Task<Ok<PaginatedList<TransferRow>>> List(ISender sender,TransferListQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<TransferDetail>> Detail(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new TransferDetailQuery(guid),ct));
    public async Task<Ok<TransferStats>> Stats(ISender sender,TransferStatsQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<PaginatedList<DispatchCandidate>>> Candidates(ISender sender,DispatchCandidatesQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Created<Guid>> Dispatch(ISender sender,DispatchInput input,CancellationToken ct){var id=await sender.Send(new DispatchTransferCommand(input),ct);return TypedResults.Created($"/Identity/Transfusion/Detail/{id}",id);}
    public async Task<Ok<Guid>> Vital(ISender sender,VitalInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new RecordTransferVitalCommand(input),ct));
    public async Task<NoContent> Receive(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Receive"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> ReceiptHold(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"ReceiptHold"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Verify(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Verify"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Witness(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Witness"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Start(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Start"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Complete(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Complete"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Stop(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Stop"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Return(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Return"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> AcceptReturn(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"AcceptReturn"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Dispose(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"Dispose"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> DoctorNote(ISender sender,TransferActionInput input,CancellationToken ct){await sender.Send(new TransferActionCommand(input,"DoctorNote"),ct);return TypedResults.NoContent();}
}
