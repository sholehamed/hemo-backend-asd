
using BotDetect.Web;
using Infrastructure.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using web.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Add services to the container.

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

builder.Services.AddAuthentication()
    ;
var app = builder.Build();




app.UseCors("AllowSpecificOrigins");
app.UseSimpleCaptcha(builder.Configuration.GetSection("BotDetect"));



await app.InitialiseDatabaseAsync();

app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });


app.MapAllGrpcServices();
app.Map("/", () => Results.Redirect("/Identity"));



app.MapEndpoints();

app.UseMiddleware<ApiRouteTemplateMiddleware>(); //
app.Run();

public partial class Program
{
}