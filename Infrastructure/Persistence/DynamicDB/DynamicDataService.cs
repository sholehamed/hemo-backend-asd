using Application.Core.DynamicData;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Extensions.DependencyInjection.Persistence
{
    public class DynamicDataService : IDynamicDataService,IDisposable
    {
        private readonly MongoClient _client;
        private IConfiguration Configuration;

        public DynamicDataService( IConfiguration Configuration)
        {
            string connectionString = Environment.GetEnvironmentVariable("Mongo_ConnectionString");
            _client = new MongoClient(connectionString);
            this.Configuration = Configuration;
        }

        public IMongoDatabase GetMongoDatabase(string databaseName)
        {
            return _client.GetDatabase(databaseName);
        }

        public IMongoDatabase GetMongoDatabase()
        {
            string MongoDdName = Configuration.GetSection("MongoDdName").Value;
            return GetMongoDatabase(MongoDdName);
        }

        public async Task<Guid> InsertAsync(string databaseName, string collectionName, BsonDocument BsonDocument)
        {
            var database = GetMongoDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var id = Guid.NewGuid();


            await collection.InsertOneAsync(BsonDocument);
            return id;
        }

        public async Task UpdateAsync(string databaseName, string collectionName, string jsonString,
            bool UpdateWorkflow)
        {
            // var database = GetMongoDatabase(databaseName);
            // var collection = database.GetCollection<BsonDocument>(collectionName);
            //
            // var updatedDoc = BsonDocument.Parse(jsonString);
            //
            // var idElement = updatedDoc.Elements
            //     .FirstOrDefault(e => e.Name.Equals("_id", StringComparison.OrdinalIgnoreCase));
            //
            // if (idElement == null)
            //     throw new ArgumentException("Document must contain '_id' field.");
            //
            // var id = idElement.Value.AsString;
            //
            // var updateDefinition = new BsonDocument();
            //
            // foreach (var element in updatedDoc.Elements)
            // {
            //     if (element.Name.Equals("_id", StringComparison.OrdinalIgnoreCase) ||
            //         element.Name.Equals("Created", StringComparison.OrdinalIgnoreCase) ||
            //         element.Name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase) ||
            //         element.Name.Equals("CreatedByIP", StringComparison.OrdinalIgnoreCase))
            //     {
            //         continue;
            //     }
            //
            //     if (UpdateWorkflow == false)
            //     {
            //         if (element.Name.Equals("UcDoneStatusGUID", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("DestWFStatusGUID", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("WFStatusName", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("WFStatusTypeGkey", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("WFStatusTypeGkey", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("WFStatusTypeName", StringComparison.OrdinalIgnoreCase) ||
            //             element.Name.Equals("WFStatusActionKey", StringComparison.OrdinalIgnoreCase))
            //         {
            //             continue;
            //         }
            //     }
            //
            //
            //     updateDefinition[element.Name.ToUpper()] = element.Value;
            // }
            //
            // updateDefinition["LastModifiedBy"] = AuthService.GetUserGUID().ToString();
            // updateDefinition["LastModifiedByIP"] = AuthService.GetIpAddress();
            // updateDefinition["LastModified"] = DateTime.Now;
            //
            // var filter = Builders<BsonDocument>.Filter.Eq("_id", id.ToLower());
            // var update = new BsonDocument("$set", updateDefinition);
            //
            // await collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteAsync(string databaseName, string collectionName, string id)
        {
            // var database = GetMongoDatabase(databaseName);
            // var collection = database.GetCollection<BsonDocument>(collectionName);
            // var filter = Builders<BsonDocument>.Filter.Eq("_id", id.ToLower());
            // await collection.DeleteOneAsync(filter);
        }

        public Task<List<T>> FindAllAsync<T>(string databaseName, string collectionName)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedList<T>> FindPageingAsync<T>(string databaseName, string collectionName,
            PageFilter<object, T> PageFilter) where T : class
        {
            var database = GetMongoDatabase(databaseName);
            var collection = database.GetCollection<T>(collectionName);

            var filter = Builders<T>.Filter.Empty;

            foreach (var Filter in PageFilter.Filters)
            {
                if (string.IsNullOrWhiteSpace(Filter.Value)) continue;
                Filter.Value = Filter.Value.Trim();

                Filter.OperatorType = Filter.OperatorType.ToLower() switch
                {
                    "startswith" => "StartsWith",
                    "contains" => "Contains",
                    "notcontains" => "NotContains",
                    "endswith" => "EndsWith",
                    "equals" => "==",
                    "notequals" => "!=",
                    "lt" => "<",
                    "lte" => "<=",
                    "gt" => ">",
                    "gte" => ">=",
                    "dateisnot" => "!=",
                    "dateis" => "==",
                    "datebefore" => "<=",
                    "dateafter" => ">=",
                    _ => Filter.OperatorType
                };

                Filter.OperatorType = Filter.OperatorType switch
                {
                    "NotContains" => "Contains", // فقط با negation
                    _ => Filter.OperatorType
                };

                FilterDefinition<T> currentFilter = Filter.OperatorType switch
                {
                    "StartsWith" => Builders<T>.Filter.Regex(Filter.FilterBy,
                        new BsonRegularExpression($"^{Filter.Value}", "i")),
                    "Contains" => Builders<T>.Filter.Regex(Filter.FilterBy,
                        new BsonRegularExpression(Filter.Value, "i")),
                    "==" => Builders<T>.Filter.Eq(Filter.FilterBy, Filter.Value),
                    "!=" => Builders<T>.Filter.Ne(Filter.FilterBy, Filter.Value),
                    ">" => Builders<T>.Filter.Gt(Filter.FilterBy, Filter.Value),
                    ">=" => Builders<T>.Filter.Gte(Filter.FilterBy, Filter.Value),
                    "<" => Builders<T>.Filter.Lt(Filter.FilterBy, Filter.Value),
                    "<=" => Builders<T>.Filter.Lte(Filter.FilterBy, Filter.Value),
                    _ => Builders<T>.Filter.Empty
                };

                if (Filter.OperatorType == "NotContains")
                    currentFilter = Builders<T>.Filter.Not(currentFilter);

                filter = Builders<T>.Filter.And(filter, currentFilter);
            }

            var totalCount = await collection.CountDocumentsAsync(filter);

            var find = collection.Find(filter);
            if (!string.IsNullOrEmpty(PageFilter.SortBy))
            {
                var sortDefinition = PageFilter.IsDescending
                    ? Builders<T>.Sort.Descending(PageFilter.SortBy)
                    : Builders<T>.Sort.Ascending(PageFilter.SortBy);
                find = find.Sort(sortDefinition);
            }

            var documents = await find.Skip((PageFilter.PageNumber - 1) * PageFilter.PageSize)
                .Limit(PageFilter.PageSize)
                .ToListAsync();

          

            var paginatedList = new PaginatedList<T>(documents, (int)totalCount, PageFilter.PageNumber, PageFilter.PageSize);

            return paginatedList;
        }

        public async Task<PaginatedList<T>> FindPageingAsync<T>(string collectionName, PageFilter<object, T> PageFilter)
            where T : class
        {
            string MongoDdName = Configuration.GetSection("MongoDdName").Value;
            return await FindPageingAsync(MongoDdName,collectionName,PageFilter);
        }

        public async Task<List<object>> FindAllAsync(string databaseName, string collectionName)
        {
            var database = GetMongoDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var allDocuments = await collection.Find(new BsonDocument()).ToListAsync();
            var list = allDocuments.Select(d => BsonTypeMapper.MapToDotNetValue(d)).ToList();
            return list;
        }


        public async Task<object> FindByIdAsync(string databaseName, string collectionName, string id)
        {
            var database = GetMongoDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", id.ToLower());
            var document = await collection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
                return null;

            return BsonTypeMapper.MapToDotNetValue(document);
        }

        public void Dispose()
        {
            _client.Dispose();
            
        }
    }
}