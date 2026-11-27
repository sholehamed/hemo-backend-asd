
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

public class ExceptionLog
{
    [BsonId]
    [JsonIgnore]
    public ObjectId Id { get; set; }
    public DateTime Created { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string Scheme { get; set; }
    public string Host { get; set; }
    public string UserAgent { get; set; }
    public string UserIp { get; set; }
    public string UserId { get; set; }
    public string ExceptionMessage { get; set; }
    public string StackTrace { get; set; }
    public string InnerException { get; set; }
}