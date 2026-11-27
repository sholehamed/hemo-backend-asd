namespace Application.Common.Models;

public class UserToken
{
    public Guid UserGUID { get; set; }
    public Guid TokenKey { get; set; }
    public DateTime ExpireDate { get; set; }
}