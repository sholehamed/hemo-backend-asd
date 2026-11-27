using MongoDB.Bson;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public static class BsonSafeConverter
{
    public static BsonDocument ToSafeBson(PropertyValues values)
    {
        var doc = new BsonDocument();

        if (values == null)
            return doc;

        foreach (var property in values.Properties)
        {
            try
            {
                var value = values[property];
                doc.Add(property.Name, ToSafeBsonValue(value));
            }
            catch
            {
                doc.Add(property.Name, BsonNull.Value);
            }
        }

        return doc;
    }

    private static BsonValue ToSafeBsonValue(object value)
    {
        if (value == null)
            return BsonNull.Value;

        var type = value.GetType();
        
        if (type.IsPrimitive || value is string || value is decimal)
            return BsonValue.Create(value);
        
        if (value is DateTime dt)
            return new BsonDateTime(dt);
        
        if (value is Guid guid)
            return new BsonString(guid.ToString());
        
        if (type.IsEnum)
            return new BsonString(value.ToString());
        
        if (value is System.Collections.IEnumerable enumerable && !(value is string))
        {
            var array = new BsonArray();
            foreach (var item in enumerable)
            {
                array.Add(ToSafeBsonValue(item));
            }
            return array;
        }
        
        return value.ToBsonDocument();
    }
}