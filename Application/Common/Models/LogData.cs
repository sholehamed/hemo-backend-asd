using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

public class LogData
{
    private const string Secret = "hjg@#!$@#hjqwgdsv#@%%$@#wasdf";

    // ------------------ Mongo ------------------
    [BsonId]
    [JsonIgnore]
    public ObjectId Id { get; set; }

    // ------------------ Data ------------------
    public string GUID { get; set; }

    [JsonIgnore]
    public BsonDocument? Befor { get; set; }

    [JsonIgnore]
    public BsonDocument? After { get; set; }

    [BsonIgnore]
    public string? BeforJson => Befor?.ToJson();

    [BsonIgnore]
    public string? AfterJson => After?.ToJson();

    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedByIP { get; set; }
    public string TableName { get; set; }
    public string LogType { get; set; }

    // ------------------ Sign ------------------
    public string Sign { get; private set; }

    // ------------------ Auto Sign ------------------
    [BsonIgnore]
    [JsonIgnore]
    private bool _skipSign;

    /// <summary>
    /// Call this before save
    /// </summary>
    public void AutoSign()
    {
        Sign = GenerateSign();
    }

    /// <summary>
    /// Check integrity after load
    /// </summary>
    [BsonIgnore]
    public bool SignIsValid => Sign == GenerateSign();

    // ------------------ Core Logic ------------------
    private string GenerateSign()
    {
        var doc = this.ToBsonDocument();
        doc.Remove("_id");
        doc.Remove("Sign");

        var json = doc.ToJson() + Secret;

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));

        return Convert.ToHexString(hash).ToLower();
    }

    // ------------------ Deserialize Hook ------------------
    [BsonConstructor]
    public LogData(
        string guid,
        DateTime created,
        string createdBy,
        string createdByIP,
        string tableName,
        string logType,
        string sign)
    {
        GUID = guid;
        Created = created;
        CreatedBy = createdBy;
        CreatedByIP = createdByIP;
        TableName = tableName;
        LogType = logType;
        Sign = sign;
    }

    public LogData() { }
}