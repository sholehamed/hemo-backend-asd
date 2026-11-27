namespace Application.Repository.Services;

public class SubSystemService : BaseRepository<SubSystem>
{
    public SubSystemService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task InserDefaults(CancellationToken cancellationToken)
    {
        var IdentityGuid = Guid.Parse("41e8f63b-ac36-44bb-9bae-2d3355b1b743");
        if (!Table.Any())
        {
            List<string> Identity = Environment.GetEnvironmentVariable("IdentityUrl")
                .Replace("http://", "").Replace("https://", "").Replace("//", "")
                .Split(":").ToList();
            Table.Add(new SubSystem
            {
                GUID = IdentityGuid,
                Name = "مدیریت کاربران",
                Port = int.Parse(Identity[1]),
                Host = Identity[0],
                Active = true
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}