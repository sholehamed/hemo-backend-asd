using System.Security.Cryptography;
using System.Text;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("نقش کاربر")]
public class UserRole : AuditedEntity
{
    private static long MaxUserRoleGkey = 0;

    [PropConfig("کاربر")] public Guid UserGUID { get; set; }
    [PropConfig("نقش")] public Guid RoleGUID { get; set; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }
    [PropConfig("تاریخ انقضا")] public DateTime ExpireDate { get; set; }
    [PropConfig("کد", IsUnique = true)] public long Gkey { get; set; }
    [PropConfig("اعتبار سنجی")] public string Sign { get; set; }


    public virtual Role Role { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual OrgLocation OrgLocation { get; set; } = null!;

    public override async Task OnAdd(DbContext Context)
    {
        await SetGkey(Context);
        await SetSign();
    }

    public override async Task OnModified(DbContext Context)
    {
        await SetSign();
    }

    private async Task<string> GetSign()
    {
        string _Sign = GUID.ToString() + UserGUID.ToString() + RoleGUID.ToString() + OrgLocationGUID + ExpireDate +
                       Gkey + "7k8LmCDeLb7eIjUQ0Isva3QOGyRsxXYR83Hi0e4FelQT9UyiSC";
        return SHA256Hash(_Sign);
    }

    private async Task SetSign()
    {
        Sign = await GetSign();
    }

    public async Task<bool> SignIsValid()
    {
        string _Sign = await GetSign();
        return _Sign == Sign;
    }

    private string SHA256Hash(string Text)
    {
        using (var sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Text));

            // Convert byte array to a string   
            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }

    public async Task SetGkey(DbContext context)
    {
        long MaxGkey = 0;
        try
        {
            if (MaxUserRoleGkey == 0)
            {
                MaxGkey = context.Set<UserRole>().IgnoreQueryFilters().Max(a => a.Gkey);
            }
            else
            {
                MaxGkey = MaxUserRoleGkey;
            }

            MaxGkey++;
            MaxUserRoleGkey = MaxGkey;
        }
        catch (Exception e)
        {
        }

        MaxGkey++;

        Gkey = MaxGkey;
    }
}