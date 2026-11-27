using System.Data;
using System.Text;
using Application.Common.Interfaces;
using Application.Core.DynamicData;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var DBNAME = "ASD_IdentityServer";
        var DbType = Environment.GetEnvironmentVariable("DbType") ?? "1";

        var UseNpgsqlConnectionString =
            $"Server={Environment.GetEnvironmentVariable("DataBaseServer")};Port={Environment.GetEnvironmentVariable("DataBasePort")};Database={DBNAME};Username={Environment.GetEnvironmentVariable("DataBaseUsername")};Password={Environment.GetEnvironmentVariable("DataBasePassword")};";
        //var SqlConnectionString =
        //    $"Server={Environment.GetEnvironmentVariable("DataBaseServer")};Database={DBNAME};User Id={Environment.GetEnvironmentVariable("DataBaseUsername")};password={Environment.GetEnvironmentVariable("DataBasePassword")};MultipleActiveResultSets=true;TrustServerCertificate=True";

        var SqlConnectionString2 =
         $"Server={Environment.GetEnvironmentVariable("DataBaseServer")};Database={DBNAME};Trusted_Connection=True;TrustServerCertificate=True";


        //if (!builder.Environment.IsProduction())
        //{
        //    SqlConnectionString = $"Server={"172.16.22.4\\mapna"};Database={DBNAME};User Id={"sa"};password={"ASDasd123456"};MultipleActiveResultSets=true;TrustServerCertificate=True".Replace("\\\\", "\\");

        //}
        //var SqlConnectionString =
        //    $"Server=192.168.3.7;Database={DBNAME};User Id={Environment.GetEnvironmentVariable("DataBaseUsername")};password=ASDasd159753@;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var SqlConnectionString =
            $"Server=.;Database=Hemo;User Id=sa;password=H@med24121373;MultipleActiveResultSets=true;TrustServerCertificate=True";

        bool hasLogging = builder.Configuration.GetValue<bool>("HasLogging");
        if (hasLogging)
            builder.Services.AddSingleton<IDynamicDataService, DynamicDataService>();

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            switch (DbType)
            {
                case "1":
                    {
                        options.UseNpgsql(UseNpgsqlConnectionString);
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        break;
                    }
                case "2":
                    {
                        options.UseSqlServer(SqlConnectionString);
                        break;
                    }
                case "3":
                    {
                        options.UseSqlServer(SqlConnectionString2);
                        break;
                    }
            }
            // 

            //options.UseSqlite(connectionString);
        });

        builder.Services.AddTransient<IDbConnection>(_ =>
            new SqlConnection(SqlConnectionString));
        builder.Services.AddScoped<DapperRepository, DapperRepository>();


        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Environment.GetEnvironmentVariable("ValidIssuer"),
                ValidAudience = Environment.GetEnvironmentVariable("ValidAudience"),
                IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("IssuerSigningKey"))),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddSingleton(TimeProvider.System);
    }
}