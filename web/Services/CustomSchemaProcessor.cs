using System.Reflection;
using Domain.Attributes;
using NJsonSchema;
using NJsonSchema.Generation;

namespace web.Services;

public class CustomSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.ContextualType.Type;

        var classNameAttribute = type.GetCustomAttribute<ClassName>();
        if (classNameAttribute != null)
        {
            context.Schema.ExtensionData ??= new Dictionary<string, object>();
            context.Schema.ExtensionData["ClassName"] = classNameAttribute.Name;
        }


        // بررسی PropNameAttribute برای هر Property
        foreach (var property in type.GetProperties())
        {
            var propNameAttribute = property.GetCustomAttribute<PropConfig>();
            if (propNameAttribute != null)
            {
                var propertyName = context.Schema.Properties.Keys.FirstOrDefault(k =>
                    k.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (propertyName != null)
                {
                    context.Schema.Properties[propertyName].ExtensionData ??= new Dictionary<string, object>();
                    context.Schema.Properties[propertyName].ExtensionData["PropName"] = propNameAttribute.Name;
                    context.Schema.Properties[propertyName].ExtensionData["MaxLenght"] = propNameAttribute.MaxLenght;
                    context.Schema.Properties[propertyName].ExtensionData["MinLenght"] = propNameAttribute.MinLenght;
                    context.Schema.Properties[propertyName].ExtensionData["MaxNumber"] = propNameAttribute.MaxNumber;
                    context.Schema.Properties[propertyName].ExtensionData["MinNumber"] = propNameAttribute.MinNumber;
                }
                
                
            }
        }
    }
}