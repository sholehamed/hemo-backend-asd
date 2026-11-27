using Application.Common.Exceptions;
using Application.Core.DynamicData;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace web.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
    private IDynamicDataService DynamicDataService;
    private IConfiguration Configuration;

    public CustomExceptionHandler( IConfiguration Configuration,IDynamicDataService? DynamicDataService= null)
    {
        this.DynamicDataService = DynamicDataService;
        this.Configuration = Configuration;

        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(ServiceExeption), HandleServiceExeption }
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (_exceptionHandlers.ContainsKey(exceptionType))
        {
            await _exceptionHandlers[exceptionType].Invoke(httpContext, exception);
            return true;
        }

        await HandleGeneralException(httpContext, exception);
        return true;
    }

    private async Task HandleServiceExeption(HttpContext httpContext, Exception ex)
    {
        var exception = (ServiceExeption)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = exception.ErrorCode.ToString(),
            Detail = exception.Message
        });
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "100"
        });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = "404",
            Detail = exception.Message
        });
    }

    private async Task HandleGeneralException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var request = httpContext.Request;

        ExceptionLog log = new ExceptionLog
        {
            Created = DateTime.UtcNow,
            Method = request.Method,
            Path = request.Path,
            Scheme = request.Scheme,
            Host = request.Host.ToString(),
            UserAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "",
            UserIp = httpContext.Request.Headers["ClientIpAddresss"].FirstOrDefault() ?? "",
            UserId = httpContext.User.Claims.FirstOrDefault(a => a.Type == "Id")?.Value ?? "",
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace ?? "",
            InnerException = ex.InnerException?.ToString() ?? "",
        };

        string MongoDdName = Configuration.GetSection("MongoDdName").Value;

        await DynamicDataService.InsertAsync(MongoDdName, "SystemErros", log.ToBsonDocument());

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "InternalServerError",
            Type = "500",
            Detail = "خطای سرور رخ داده است"
        });
    }


    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        });
    }
}