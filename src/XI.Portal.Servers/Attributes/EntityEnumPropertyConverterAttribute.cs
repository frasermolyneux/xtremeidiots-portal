using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Servers.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityEnumPropertyConverterAttribute : Attribute
    {
        public EntityEnumPropertyConverterAttribute()
        {
            
        }
    }

    public class EntityEnumPropertyConverter
    {
        public static void Serialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> results)
        {
            entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityEnumPropertyConverterAttribute), false).Any())
                .ToList()
                .ForEach(x => results.Add(x.Name, new EntityProperty(x.GetValue(entity) != null ? x.GetValue(entity).ToString() : null)));
        }

        public static void Deserialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> properties)
        {
            entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityEnumPropertyConverterAttribute), false).Any())
                .ToList()
                .ForEach(x => x.SetValue(entity, Enum.Parse(x.PropertyType, properties[x.Name].StringValue)));
        }
    }
}