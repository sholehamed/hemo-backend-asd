namespace Application.Common.Models;

public class UserRoleInfo
{
    public Guid UserGuid { get; set; }
    public Guid RoleGuid { get; set; }
    
    public long RoleGkey { get; set; }
    public Guid OrgLocationGUID { get; set; }
    
    public long OrgLocationGkey { get; set; }
    public string OrgLocationPath { get; set; }
    public string RolePath { get; set; }
    public bool IsSuperAdmin { get; set; }
    
    public string UserFirstName { get; set; }
    
    public string UserLastName { get; set; }
    
    public string RoleName { get; set; }
    public DateTime ExpireDate { get; set; }
    
}