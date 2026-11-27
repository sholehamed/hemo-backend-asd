using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Security;
using System.Net;
using web.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();


        builder.Services.AddHttpContextAccessor();


        builder.Services.AddExceptionHandler<CustomExceptionHandler>();


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "CleanArchitecture API";

            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.SchemaSettings.SchemaProcessors.Add(new CustomSchemaProcessor());

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            configure.OperationProcessors.Add(new OperationSummaryAndDescriptionProcessor());
        });
        
        string IdentityUrl = Environment.GetEnvironmentVariable(builder.Configuration["Variables:IdentityUrl"]).ToLower().Replace("http://","").Replace("https://","");
        string IdentityGRPCUrl = Environment.GetEnvironmentVariable(builder.Configuration["Variables:IdentityGRPCUrl"]).ToLower().Replace("http://","").Replace("https://","");

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Parse(IdentityUrl.Split(":")[0]), int.Parse(IdentityUrl.Split(":")[1]), listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });
        });
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Parse(IdentityGRPCUrl.Split(":")[0]), int.Parse(IdentityGRPCUrl.Split(":")[1]), listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
        
        builder.Services.AddGrpc();
        
    }
}