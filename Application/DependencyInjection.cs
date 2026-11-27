using System.Reflection;
using Application.Common.Behaviours;
using Application.Common.Extensions;
using Application.Common.Utility.Security;
using Application.Repository;
using Application.Repository.Webservice;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        //builder.Services.AddSingleton<IConsulClient>(consul => new ConsulClient(consulConfig =>
        //{
        //    consulConfig.Address =
        //        new Uri(
        //            $"http://{Environment.GetEnvironmentVariable("ConsulUrl")}:{Environment.GetEnvironmentVariable("ConsulPort")}");
        //}));
        builder.Services.AddTransient<Sabtahval>();
        builder.Services.AddTransient<Shahkar>();
        builder.Services.AddTransient<SmsService>();
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


        builder.Services.AddScoped<AuthService, AuthService>();

        builder.Services.AddRepositories(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });
    }
}