using Domain.Common;

namespace Application.Repository;

public abstract class BaseRepository<T> where T : BaseEntity
{
    public readonly IApplicationDbContext db;
    public readonly IMapper mapper;
    public DbSet<T> Table;


    public BaseRepository(IApplicationDbContext context, IMapper mapper)
    {
        db = context;
        this.mapper = mapper;
        setDbSet();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    private void setDbSet()

    {
        Table = db.GetType().GetProperty(typeof(T).Name)!.GetValue(db) as DbSet<T>;
    }
}