using Application.Common.Utility.Security;
using Application.Core.User_core.Commands.UserLogin;
using Application.Repository.Services;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hemo.Admission.Tests;
public class HemoDemoSeedTests
{
    private sealed class Audit:SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e,InterceptionResult<int> result,CancellationToken ct=default)
        {
            foreach(var row in e.Context!.ChangeTracker.Entries<BaseEntity>())
            {
                if(row.State==EntityState.Added)row.Entity.Created=DateTime.UtcNow;
                if(row.Entity is AuditedEntity audited && row.State is EntityState.Added or EntityState.Modified)
                {
                    audited.CreatedByIP="test";audited.LastModifiedByIP="test";audited.LastModified=DateTime.UtcNow;
                }
            }
            return ValueTask.FromResult(result);
        }
    }
    [Fact]
    [Trait("Category","SqlServer")]
    public async Task SeedIsRepeatableAndRolesAreEligibleWithoutGlobalPrivileges()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_seed_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);
        var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true);
            var secrets=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodBank"]="true";
            var configuration=new ConfigurationBuilder().AddInMemoryCollection(secrets).Build();
            await HemoDemoSeed.ApplyAsync(db,configuration,ct);
            var userCount=await db.User.CountAsync(ct);
            var roleCount=await db.Role.CountAsync(ct);
            var grantCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeBloodBank","true"}}).Build(),ct);
            Assert.Equal(userCount,await db.User.CountAsync(ct));
            Assert.Equal(roleCount,await db.Role.CountAsync(ct));
            Assert.Equal(grantCount,await db.RoleAccess.CountAsync(ct));
            foreach(var account in accounts)
            {
                var user=await db.User.SingleAsync(u=>u.Username==account.Username,ct);
                Assert.Equal(UserService.HashPassword(user.GUID,secrets["HemoSeed:Passwords:"+account.Key]!),user.Password);
                var assignment=Assert.Single(await HemoLoginContext.EligibleRoles(db,user.GUID,DateTime.Now).ToListAsync(ct));
                Assert.True(assignment.Role.Gkey>2);
                var permissions=await db.RoleAccess.Where(r=>r.RoleGUID==assignment.RoleGUID).Select(r=>r.ApiGUID).ToListAsync(ct);
                Assert.Equal(account.Permissions.Length,permissions.Count);
                Assert.DoesNotContain(permissions,p=>p==null);
            }
        }
        finally {await db.Database.EnsureDeletedAsync(ct);}
    }
}
