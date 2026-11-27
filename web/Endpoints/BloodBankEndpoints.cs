using Application.Common.Models;
using Application.Repository.Services;
using Application.Common.Utility.Security;
using Application.Core.BloodBank_core;
using Application.Core.BloodBank_core.Commands;
using Application.Core.BloodBank_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Web.Endpoints;
public class BloodBank : EndpointGroupBase
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
            .MapGet(Capabilities,"Capabilities").MapPost(Inventory,"Inventory").MapGet(Unit,"Unit/{guid:guid}").MapPost(Stats,"Stats")
            .MapPost(TemperatureHistory,"TemperatureHistory").MapPost(LabHistory,"LabHistory").MapPost(Receive,"Receive").MapPost(Test,"Test").MapPost(Temperature,"Temperature").MapPost(ReturnRequest,"ReturnRequest")
            .MapPost(Inspect,"Inspect").MapPost(Reserve,"Reserve").MapPost(Thaw,"Thaw").MapPost(Ready,"Ready").MapPost(Hold,"Hold").MapPost(Unreserve,"Unreserve").MapPost(Discard,"Discard");
    }
    public async Task<Ok<BankCapabilities>> Capabilities(ISender sender,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new BankCapabilitiesQuery(),ct));
    public async Task<Ok<PaginatedList<UnitRow>>> Inventory(ISender sender,BankInventoryQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<UnitDetail>> Unit(ISender sender,Guid guid,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new BankUnitQuery(guid),ct));
    public async Task<Ok<BankStats>> Stats(ISender sender,BankStatsQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<PaginatedList<BankLabReportRow>>> LabHistory(ISender sender,BankLabHistoryQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Ok<PaginatedList<TemperatureView>>> TemperatureHistory(ISender sender,BankTemperatureQuery query,CancellationToken ct)=>TypedResults.Ok(await sender.Send(query,ct));
    public async Task<Created<Guid>> Receive(ISender sender,ReceiveUnit input,CancellationToken ct){var id=await sender.Send(new ReceiveBankUnitCommand(input),ct);return TypedResults.Created($"/Identity/BloodBank/Unit/{id}",id);}
    public async Task<Ok<Guid>> Test(ISender sender,LabInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new RecordBankLabCommand(input),ct));
    public async Task<Ok<Guid>> Temperature(ISender sender,TemperatureInput input,CancellationToken ct)=>TypedResults.Ok(await sender.Send(new RecordBankTemperatureCommand(input),ct));
    public async Task<NoContent> ReturnRequest(ISender sender,ReturnRequestInput input,CancellationToken ct){await sender.Send(new BankReturnRequestCommand(input),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Inspect(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Inspect"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Reserve(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Reserve"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Thaw(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Thaw"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Ready(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Ready"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Hold(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Hold"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Unreserve(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Unreserve"),ct);return TypedResults.NoContent();}
    public async Task<NoContent> Discard(ISender sender,UnitAction input,CancellationToken ct){await sender.Send(new BankUnitActionCommand(input,"Discard"),ct);return TypedResults.NoContent();}
}
