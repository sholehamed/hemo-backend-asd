using MongoDB.Driver;
namespace Microsoft.Extensions.DependencyInjection.Persistence;

public class LogDbContext
{
    private readonly IMongoDatabase _database;

    public LogDbContext(LogDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}