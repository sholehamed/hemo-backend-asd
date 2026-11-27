
namespace Application.Common.Interfaces;
public interface  IApplicationDbContext
{
     public  DbSet<Api> Api { get; set;}
     public  DbSet<BlockListIP> BlockListIP { get; set;}
     public  DbSet<DemoRequest> DemoRequest { get; set;}
     public  DbSet<Department> Department { get; set;}
     public  DbSet<Discount> Discount { get; set;}
     public  DbSet<Menu> Menu { get; set;}
     public  DbSet<MenuApi> MenuApi { get; set;}
     public  DbSet<OrgLocation> OrgLocation { get; set;}
     public  DbSet<OrglocationPackage> OrglocationPackage { get; set;}
     public  DbSet<OrgLocationType> OrgLocationType { get; set;}
     public  DbSet<OrgType> OrgType { get; set;}
     public  DbSet<OTP> OTP { get; set;}
     public  DbSet<Package> Package { get; set;}
     public  DbSet<PackageCategory> PackageCategory { get; set;}
     public  DbSet<PasswordBlockList> PasswordBlockList { get; set;}
     public  DbSet<Priority> Priority { get; set;}
     public  DbSet<Role> Role { get; set;}
     public  DbSet<RoleAccess> RoleAccess { get; set;}
     public  DbSet<ShoppingCart> ShoppingCart { get; set;}
     public  DbSet<ShoppingCartProduct> ShoppingCartProduct { get; set;}
     public  DbSet<ShoppingCartStatus> ShoppingCartStatus { get; set;}
     public  DbSet<sso> sso { get; set;}
     public  DbSet<SubSystem> SubSystem { get; set;}
     public  DbSet<SystemSetting> SystemSetting { get; set;}
     public  DbSet<Ticket> Ticket { get; set;}
     public  DbSet<TicketMessage> TicketMessage { get; set;}
     public  DbSet<TicketStatus> TicketStatus { get; set;}
     public  DbSet<User> User { get; set;}
     public  DbSet<UserInvite> UserInvite { get; set;}
     public  DbSet<UserRole> UserRole { get; set;}
     public  DbSet<Whitelisting> Whitelisting { get; set;}

    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

