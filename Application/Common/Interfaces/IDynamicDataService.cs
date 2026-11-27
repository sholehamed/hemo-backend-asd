using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Application.Core.DynamicData
{
    public interface IDynamicDataService
    {
        IMongoDatabase GetMongoDatabase(string databaseName);
        IMongoDatabase GetMongoDatabase();
        Task<Guid> InsertAsync(string databaseName, string tableName, BsonDocument data);
        Task UpdateAsync(string databaseName, string tableName, string data,bool UpdateWorkflow=false);
        Task DeleteAsync(string databaseName, string tableName, string id);
        Task<List<T>> FindAllAsync<T>(string databaseName, string collectionName);

        Task<PaginatedList<T>> FindPageingAsync<T>(
            string databaseName,
            string collectionName,
            PageFilter<object, T> PageFilter)where T : class;

        public  Task<PaginatedList<T>> FindPageingAsync<T>(
            string collectionName,
            PageFilter<object, T> PageFilter) where T : class;

        Task<object> FindByIdAsync(string databaseName, string collectionName, string id);
    }
}