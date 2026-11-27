using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("دسته بندی نوع شرکت")]
public class OrgType : BaseEntity
{
    [PropConfig("نوع شرکت")] public Guid OrgLocationTypeGUID { get; set; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }

    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual OrgLocationType OrgLocationType { get; set; } = null!;

}