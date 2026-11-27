using System.Security.Claims;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Application.Common.Utility.Security;

public class AuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public Guid GetUserGUID()
    {
        try
        {
            var UserId = _httpContextAccessor.HttpContext.User.Claims.First(a => a.Type == "Id")
                .Value;
            return new Guid(UserId);
        }
        catch
        {
            return Guid.Empty;
        }
    }
    public List<long> GetUserRoleGkeys()
    {
        try
        {
            List<long> UserRoles = _httpContextAccessor.HttpContext.User.Claims.Where(a => a.Type == "UserRoles").Select(a => long.Parse(a.Value)).ToList();
          
            return UserRoles;
        }
        catch
        {
            return new List<long>();
        }
    }

    public DateTime? GetTokenExpireDate()
    {
        try
        {
            var UserId = _httpContextAccessor.HttpContext.User.Claims.First(a => a.Type == "ExpireDate")
                .Value;
            return DateTime.Parse(UserId);
        }
        catch
        {
            return null;
        }
    }

    public long GetUserRoleGkey()
    {
        try
        {
            var UserRole = long.Parse(_httpContextAccessor.HttpContext.Request.Headers["UserRole"].First());
            var UserRoles = _httpContextAccessor.HttpContext.User.Claims.Where(a => a.Type == "UserRoles").ToList()
                .Select(a => long.Parse(a.Value)).ToList();

            if (UserRoles.Any(a => a == UserRole))
            {
                return UserRole;
            }
            else
            {
                throw new ServiceExeption("خطای امنیتی در نقش کاربر رخ داده است.");
            }
        }
        catch (ServiceExeption e)
        {
            throw e;
        }
        catch (Exception e)
        {
            return 0;
        }
    }

    public string GetIpAddress()
    {
        try
        {
            var IP = _httpContextAccessor.HttpContext.Request.Headers["ClientIpAddresss"].First();
            return IP;
        }
        catch (Exception e)
        {
            return "";
        }
    }
    public string GetUserAgent()
    {
        try
        {
            var UserAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].First();
            return UserAgent;
        }
        catch (Exception e)
        {
            return "";
        }
    }
    
    public string GetIpAddressFromToken()
    {
        try
        {
            var IP =_httpContextAccessor.HttpContext.User.Claims.First(a => a.Type == "IpAddress").Value;
            return IP;
        }
        catch (Exception e)
        {
            return "";
        }
    }
    public string GetUserAgentFromToken()
    {
        try
        {
            var UserAgent = _httpContextAccessor.HttpContext.User.Claims.First(a => a.Type == "UserAgent").Value;
            return UserAgent;
        }
        catch (Exception e)
        {
            return "";
        }
    }
}