using Dapper;
using System.Data;
using System.Reflection;

namespace Application.Common.Interfaces;

public class DapperRepository:IDisposable
{
    private readonly IDbConnection _connection;

    public DapperRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<T>> GetAllAsync<T>(string Query, DynamicParameters dynamicParameters)
    {
        try
        {
            var result = await _connection.QueryAsync<T>(Query, dynamicParameters);
            return result.ToList();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}