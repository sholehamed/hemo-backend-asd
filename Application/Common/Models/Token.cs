namespace Application.Common.Models;

public class Token
{
    public string Key { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime ExpireDate { get; set; }
    public bool ForceChangePassword { get; set; }
    public bool MultiLogin { get; set; }
    public long UserRole { get; set; }
}