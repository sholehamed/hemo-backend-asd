using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("نوع شرکت")]
public class OrgLocationType : BaseEntity
{
    [PropConfig("عنوان")] public string Name { get; set; }
    public virtual ICollection<OrgType> OrgTypes { get; set; } = new List<OrgType>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<OrgLocation> OrgLocations { get; set; } = new List<OrgLocation>();
}