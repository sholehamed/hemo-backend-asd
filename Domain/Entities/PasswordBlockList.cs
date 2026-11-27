using Domain.Attributes;

namespace Domain.Entities;

[ClassName("پسورد های مسدود شده")]
public class PasswordBlockList:BaseEntity
{
    [PropConfig("پسورد",HasIndex:true)]
    public string Password { get; set; }
}