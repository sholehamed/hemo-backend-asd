using Application.Common.Models;
using Application.Core.User_core.Commands.UserLogin;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web.Endpoints;

public class HemoSession : EndpointGroupBase
{
    public override void Map(WebApplication app) {
        var group=app.MapGroup(this).AllowAnonymous();
        group.MapPost("Login", Login).WithName("HEMO.HemoSession.Login");
    }

    public async Task<Ok<Token>> Login(ISender sender, UserLoginCommand command, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(command.ForHemo(),ct));
}
