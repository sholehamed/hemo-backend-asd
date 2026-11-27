using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.HTTP;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Repository.Services;

public class ApiService : BaseRepository<Api>
{
    private IServiceScopeFactory IServiceScopeFactory;


    public ApiService(IApplicationDbContext context, IMapper mapper, IServiceScopeFactory iServiceScopeFactory) : base(
        context, mapper)
    {
        IServiceScopeFactory = iServiceScopeFactory;
    }

    public async Task UpateApiListInBackground()
    {
        //using (var scope = IServiceScopeFactory.CreateScope())
        //{
        //    ApiService ApiService = scope.ServiceProvider.GetRequiredService<ApiService>();
        //    await ApiService.UpateApiList();
        //}
    }

    public async Task<bool> UpateApiList()
    {
        List<Api> apis = new();
        List<SubSystem> subSystems = db.SubSystem.Where(a => a.Active).ToList();
        foreach (var subSystem in subSystems)
        {
            try
            {
                List<Api> subSystemApis = await GetApiFromSwagger(subSystem);
                apis.AddRange(subSystemApis);
            }
            catch (Exception)
            {
                subSystem.Active = false;
                //throw new ServiceExeption("سرویس " + subSystem.Name + " در دسترس نمی باشد.");
            }
        }
        
        var minPriority = (apis.Any()) ? apis.Min(a => a.Priority) : 0;
        apis = apis.Select(api =>
        {
            api.Priority -= minPriority;
            return api;
        }).ToList();
        
        await SaveApis(apis);
        await SetApisToGateway();
        return true;
    }

    private async Task<List<Api>> GetApiFromSwagger(SubSystem subSystem)
    {
        var apis = new ConcurrentBag<Api>();
        var url = $"http://{subSystem.Host}:{subSystem.Port}/api/specification.json";
        string jsonResult;

        jsonResult = await HTTP.Get(url, Timeout: 5);
        if (string.IsNullOrWhiteSpace(jsonResult))
        {
            return await GetApiFromOldSwagger(subSystem);
        }

        var jsonObject = JObject.Parse(jsonResult);
        var paths = jsonObject["paths"];

        Parallel.ForEach(paths, path =>
        {
            var pathKey = path.Path.Replace("paths['", "").Replace("']", "");
            var methods = path.First;

            Parallel.ForEach(methods.Children<JProperty>(), method =>
            {
                var methodName = method.Name;
                var description = method.First["description"];

                var api = new Api
                {
                    Priority = 10000,
                    DownstreamPathTemplate = pathKey,
                    UpstreamPathTemplate = pathKey,
                    IsPublic = false,
                    APITypeName = methodName,
                    SubSystemGUID = subSystem.GUID,
                    Active = true,
                    Title = description?.ToString()
                };

                if (pathKey.Contains("{"))
                {
                    var sections = pathKey.Split('/').Reverse().ToList();
                    for (var i = 0; i < sections.Count; i++)
                    {
                        var score = sections[i].Count(c => c == '{') * (i + 1);
                        api.Priority -= score;
                    }
                }

                apis.Add(api);
            });
        });
        if (subSystem.GUID == Guid.Parse("41E8F63B-AC36-44BB-9BAE-2D3355B1B743"))
        {
            var api = new Api
            {
                Priority = 1,
                DownstreamPathTemplate = "/captchaendpoint",
                UpstreamPathTemplate = "/captchaendpoint",
                IsPublic = true,
                APITypeName = "get",
                SubSystemGUID = subSystem.GUID,
                Active = true,
                Title = "کپچا"
            };
            apis.Add(api);
        }

        return apis.ToList();
    }

    private async Task<List<Api>> GetApiFromOldSwagger(SubSystem subSystem)
    {
        var apis = new ConcurrentBag<Api>();
        var url = $"http://{subSystem.Host}:{subSystem.Port}/swagger/v1/swagger.json";
        string jsonResult;

        try
        {
            jsonResult = await HTTP.Get(url, Timeout: 5);
        }
        catch
        {
            throw new ServiceExeption($"سرویس {subSystem.Name} در دسترس نمی‌باشد.");
        }

        var jsonObject = JObject.Parse(jsonResult);
        var paths = jsonObject["paths"];

        Parallel.ForEach(paths, path =>
        {
            var pathKey = path.Path.Replace("paths['", "").Replace("']", "");
            var methods = path.First;

            Parallel.ForEach(methods.Children<JProperty>(), method =>
            {
                var methodName = method.Name;
                var description = method.First["description"];

                var api = new Api
                {
                    Priority = 10000,
                    DownstreamPathTemplate = pathKey,
                    UpstreamPathTemplate = pathKey,
                    IsPublic = false,
                    APITypeName = methodName,
                    SubSystemGUID = subSystem.GUID,
                    Active = true,
                    Title = "نامشخص"
                };

                if (pathKey.Contains("{"))
                {
                    var sections = pathKey.Split('/').Reverse().ToList();
                    for (var i = 0; i < sections.Count; i++)
                    {
                        var score = sections[i].Count(c => c == '{') * (i + 1);
                        api.Priority -= score;
                    }
                }

                apis.Add(api);
            });
        });


        return apis.ToList();
    }

    private async Task<bool> SaveApis(List<Api> Apis, CancellationToken cancellationToken = default)
    {
        List<Api> OldApis = db.Api.IgnoreQueryFilters().Where(a => a.SubSystem.Active).ToList();

        var apisToAdd = new ConcurrentBag<Api>();
        var apisToRemove = new ConcurrentBag<Api>();


        Parallel.ForEach(Apis, _Api =>
        {
            var OldApi = OldApis.FirstOrDefault(item =>
                //item.UpstreamPathTemplate == _Api.UpstreamPathTemplate &&
                item.DownstreamPathTemplate == _Api.DownstreamPathTemplate &&
                item.APITypeName == _Api.APITypeName
            );

            if (OldApi == null)
            {
                _Api.Internet = true;
                _Api.Intranet = true;
                apisToAdd.Add(_Api);
            }

            else if (OldApi.IsDelete)
            {
                OldApi.IsDelete = false;
                OldApi.Deleted = null;
                OldApi.DeletedBy = null;
                OldApi.DeletedByIP = null;
            }
        });

        Parallel.ForEach(OldApis, _Api =>
        {
            var OldApiExistInNewList = Apis.Any(item =>
                //  item.UpstreamPathTemplate == _Api.UpstreamPathTemplate &&
                item.DownstreamPathTemplate == _Api.DownstreamPathTemplate &&
                item.APITypeName == _Api.APITypeName);

            if (!OldApiExistInNewList)
            {
                apisToRemove.Add(_Api);
            }
        });


        if (apisToRemove.Any())
        {
            Table.RemoveRange(apisToRemove);
        }

        if (apisToAdd.Any())
        {
            Table.AddRange(apisToAdd);
        }

        List<Api> Invalids = apisToAdd.Where(a => string.IsNullOrWhiteSpace(a.Title)).ToList();
        if (Invalids.Any())
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("لطفا ابتدا مشکل سرویس های زیر را حل کنید");
            foreach (var Invalid in Invalids)
            {
                stringBuilder.AppendLine(Invalid.DownstreamPathTemplate);
            }

            throw new ServiceExeption(stringBuilder.ToString());
        }

        await SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> SetApisToGateway()
    {
        List<Api> apis = Table.Include(api => api.SubSystem).Where(a =>
        //  a.SubSystemGUID==Guid.Parse("41E8F63B-AC36-44BB-9BAE-2D3355B1B743")&&
            a.Active && a.SubSystem.Active).ToList();
        var ocelotConfig = new OcelotConfig();

        var RoleAccessList = db.RoleAccess
            .Select(a => new { RoleGkey = a.Role.Gkey.ToString(), a.ApiGUID }).ToList();

        var RoleAccessList2 = db.RoleAccess.Where(a => a.ApiGUID == null)
            .Select(a => new { RoleGkey = a.Role.Gkey.ToString(), ApiGUIDs = a.Menu.MenuApis.Select(b => b.ApiGUID) })
            .ToList();

        Parallel.ForEach(apis, api =>
        {
            List<string> AllowedScopes = RoleAccessList.Where(a => a.ApiGUID == api.GUID)
                .Select(a => a.RoleGkey).ToList();

            List<string> AllowedScopes2 = RoleAccessList2
                .Where(a => a.ApiGUIDs.Any(b => b == api.GUID))
                .Select(a => a.RoleGkey).ToList();

            AllowedScopes.AddRange(AllowedScopes2);
            AllowedScopes.Add("1");
            AllowedScopes = AllowedScopes.Distinct().ToList();

            
            var route = new Route
            {
                Priority = api.Priority,
                DownstreamPathTemplate = api.DownstreamPathTemplate,
                UpstreamPathTemplate = api.UpstreamPathTemplate,
                UpstreamHttpMethod = new List<string> { api.APITypeName.ToUpper() },
                AuthenticationOptions = new AuthenticationOptions(),
                RateLimitOptions = new RateLimitOptions()
                {
                    Limit = api.Limit??0,
                    Period =(api.EnableRateLimiting)? api.Period + "s":"",
                    EnableRateLimiting = api.EnableRateLimiting,
                    PeriodTimespan = api.Period??0,
                    
                },
                Metadata = new Metadata()
                {
                    ApiId = api.GUID,
                    Internet = api.Internet,
                    Intranet = api.Intranet,
                    LogData = api.LogData,
                    Sensitive = api.Sensitive ?? "",
                    LogResponseHeaders = api.LogResponseHeaders,
                    LogRequestBody = api.LogRequestBody,
                    LogRequestHeaders = api.LogRequestHeaders,
                    LogResponseBody = api.LogResponseBody,
                    StartTime = api.SubSystem.StartTime,
                    EndTime = api.SubSystem.EndTime
                }
            };
            if (!api.IsPublic)
            {
                route.AuthenticationOptions = new AuthenticationOptions
                {
                    AuthenticationProviderKey = "BearerApi",
                    AllowedScopes = AllowedScopes
                };
            }

            route.DownstreamHostAndPorts = new List<DownstreamHostAndPort>
            {
                new()
                {
                    Host = api.SubSystem.Host,
                    Port = api.SubSystem.Port
                }
            };

            if (api.SubSystem.Port == 443)
                route.DownstreamScheme = "https";
            else
                route.DownstreamScheme = "http";

            ocelotConfig.Routes.Add(route);
        });

        List<string>BlockListIP=db.BlockListIP.Select(a=>a.IPAddress).Distinct().ToList().Distinct().ToList();
        List<string>Whitelisting=db.Whitelisting.Select(a=>a.IPAddress).Distinct().ToList().Distinct().ToList();

        ocelotConfig.GlobalConfiguration = new GlobalConfiguration()
        {
            SecurityOptions = new SecurityOptions()
            {
                IPAllowedList = Whitelisting,
                IPBlockedList = BlockListIP
            }
        };


        var GatewayUrl = Environment.GetEnvironmentVariable("GatewayUrl");

        var GatewayToken = await CreateGatewayToken();
        await HTTP.Post(GatewayUrl + "/9qi3db67q8fjcnhulypvb8u/configuration", ocelotConfig, GatewayToken);


        return true;
    }

    public async Task<string> CreateGatewayToken()
    {
        var issuer = Environment.GetEnvironmentVariable("ValidIssuer");
        var audience = Environment.GetEnvironmentVariable("ValidAudience");
        var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("AdminIssuerSigningKey"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddYears(10),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return jwtToken;
    }

    public async Task TestHost(string Host, int Port)
    {
        try
        {
            var Url = $"http://{Host}:{Port}/api/specification.json";
            var JsonResult = await HTTP.Get(Url, Timeout: 5);
        }
        catch (Exception e)
        {
            throw new ServiceExeption("ادرس وارد شده در دسترس نمی باشد");
        }
    }
}