using Application.Common.Utility.Security;
using Application.Core.DynamicData;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Persistence;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly AuthService AuthService;
    private IDynamicDataService DynamicDataService;
    private IConfiguration Configuration;

    public AuditableEntityInterceptor(AuthService authService, IConfiguration configuration, IDynamicDataService? dynamicDataService=null)
    {
        AuthService = authService;
        DynamicDataService = dynamicDataService;
        Configuration = configuration;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        List<LogData> LogData = UpdateEntities(eventData.Context);

        var Result = base.SavingChanges(eventData, result);
        string MongoDdName = Configuration.GetSection("MongoDdName").Value;
        foreach (var Log in LogData)
        {
            DynamicDataService.InsertAsync(MongoDdName,"Log_"+ Log.TableName, Log.ToBsonDocument());
        }

        return Result;
    }

    public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        List<LogData> LogData = UpdateEntities(eventData.Context);

        var Result = await base.SavingChangesAsync(eventData, result, cancellationToken);
        string MongoDdName = Configuration.GetSection("MongoDdName").Value;
        foreach (var Log in LogData)
        {
            DynamicDataService.InsertAsync(MongoDdName, "Log_"+Log.TableName, Log.ToBsonDocument());
        }

        return Result;
    }

    public List<LogData> UpdateEntities(DbContext? context)
    {
        if (context == null) new List<JObject>();

        List<LogData> LogDatas = new List<LogData>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = DateTime.Now;
                LogData logData = new LogData()
                {
                    Befor = null,
                    After = BsonSafeConverter.ToSafeBson(entry.CurrentValues),
                    GUID = entry.Entity.GUID.ToString(),
                    Created = DateTime.UtcNow,
                    CreatedBy = AuthService.GetUserGUID().ToString(),
                    CreatedByIP = AuthService.GetIpAddress(),
                    TableName = entry.Metadata.GetTableName(),
                    LogType = "ADD"
                };
                logData.AutoSign();
                LogDatas.Add(logData);
            }

            if (entry.State == EntityState.Modified)
            {
                LogData logData = new LogData()
                {
                    Befor = BsonSafeConverter.ToSafeBson(entry.OriginalValues),
                    After = BsonSafeConverter.ToSafeBson(entry.CurrentValues),
                    GUID = entry.Entity.GUID.ToString(),
                    Created = DateTime.UtcNow,
                    CreatedBy = AuthService.GetUserGUID().ToString(),
                    CreatedByIP = AuthService.GetIpAddress(),
                    TableName = entry.Metadata.GetTableName(),
                    LogType = "EDIT"
                };
                logData.AutoSign();
                LogDatas.Add(logData);
            }


            if (entry.State == EntityState.Deleted)
            {
                LogData logData = new LogData()
                {
                    Befor = BsonSafeConverter.ToSafeBson(entry.OriginalValues),
                    After = null,
                    GUID = entry.Entity.GUID.ToString(),
                    Created = DateTime.UtcNow,
                    CreatedBy = AuthService.GetUserGUID().ToString(),
                    CreatedByIP = AuthService.GetIpAddress(),
                    TableName = entry.Metadata.GetTableName(),
                    LogType = "DELETE"
                };
                logData.AutoSign();
                LogDatas.Add(logData);
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<CreateAuditedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = AuthService.GetUserGUID();
                entry.Entity.CreatedByIP = AuthService.GetIpAddress();
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ModificationAuditedEntity>())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                entry.Entity.LastModifiedBy = AuthService.GetUserGUID();
                entry.Entity.LastModifiedByIP = AuthService.GetIpAddress();
                entry.Entity.LastModified = DateTime.Now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditedEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.Entity.DeletedBy = AuthService.GetUserGUID();
                entry.Entity.DeletedByIP = AuthService.GetIpAddress();
                entry.Entity.Deleted = DateTime.Now;
                entry.Entity.IsDelete = true;
                entry.State = EntityState.Modified;
            }
        }

        return LogDatas;
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry)
    {
        return entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}