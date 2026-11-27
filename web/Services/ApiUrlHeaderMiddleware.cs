using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

public class ApiRouteTemplateMiddleware
{
    private readonly RequestDelegate _next;

    public ApiRouteTemplateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var endpoint = context.GetEndpoint();

            if (endpoint is RouteEndpoint routeEndpoint)
            {
                // این همان چیزی است که تو می‌خواهی
                var template = routeEndpoint.RoutePattern.RawText;

                context.Response.Headers["X-Api-Route"] = template;
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}