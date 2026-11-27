using Application.Common.Utility.Security;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
namespace Hemo.Admission.Tests;
public class AdmissionModelTests
{
    private static ApplicationDbContext Context(string connection)=>new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection).Options,
        new AuthService(new HttpContextAccessor()));
    [Fact]
    public void ModelUsesUniqueIdentityAndAdmissionIndexesAndConcurrency()
    {
        using var db=Context("Server=localhost;Database=HemoModelContract;Integrated Security=True;TrustServerCertificate=True");
        var patient=db.Model.FindEntityType(typeof(Patient))!;
        var admission=db.Model.FindEntityType(typeof(PatientAdmission))!;
        Assert.Contains(patient.GetIndexes(),i=>i.IsUnique&&i.Properties.Any(p=>p.Name=="IdentificationNumber")&&i.Properties.Any(p=>p.Name=="OrgLocationGUID"));
        Assert.Contains(admission.GetIndexes(),i=>i.IsUnique&&i.Properties.Any(p=>p.Name=="HISNumber")&&i.Properties.Any(p=>p.Name=="OrgLocationGUID"));
        Assert.True(admission.FindProperty(nameof(PatientAdmission.Version))!.IsConcurrencyToken);
        Assert.Contains(admission.GetForeignKeys(),f=>f.PrincipalEntityType.ClrType==typeof(Patient));
        Assert.NotNull(db.Model.FindEntityType(typeof(PatientAdmissionReview)));
    }
    [Fact]
    public void NewMigrationCanGenerateSql()
    {
        using var db=Context("Server=localhost;Database=HemoModelContract;Integrated Security=True;TrustServerCertificate=True");
        var migrations=db.Database.GetMigrations().ToArray();
        var index=Array.IndexOf(migrations,"20261127180000_AddHemoAdmission");
        Assert.True(index>0);
        var sql=db.GetService<IMigrator>().GenerateScript(migrations[index-1],migrations[index]);
        Assert.Contains("CREATE TABLE [Patient]",sql);
        Assert.Contains("CREATE TABLE [PatientAdmission]",sql);
        Assert.Contains("CREATE TABLE [PatientAdmissionReview]",sql);
        Assert.DoesNotContain("DROP TABLE",sql);
    }
    [Fact]
    [Trait("Category","SqlServer")]
    public async Task MigrationsApplyOnIsolatedDatabase()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection),"Set HEMO_TEST_SQL to an isolated test server connection.");
        var builder=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST02_"+Guid.NewGuid().ToString("N")};
        await using var db=Context(builder.ConnectionString);
        try
        {
            await db.Database.MigrateAsync();
            Assert.Contains("20261127180000_AddHemoAdmission",await db.Database.GetAppliedMigrationsAsync());
            Assert.Empty(await db.Patient.AsNoTracking().ToListAsync());
            Assert.Empty(await db.PatientAdmission.AsNoTracking().ToListAsync());
            Assert.Empty(await db.PatientAdmissionReview.AsNoTracking().ToListAsync());
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
}
