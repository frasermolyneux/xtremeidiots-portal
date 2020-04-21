using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using XI.AzureTableExtensions.Attributes;

namespace XI.AzureTableExtensions
{
    public class TableEntityExtended : TableEntity
    {
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityJsonPropertyConverter.Serialize(this, results);
            EntityEnumPropertyConverter.Serialize(this, results);
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityJsonPropertyConverter.Deserialize(this, properties);
            EntityEnumPropertyConverter.Deserialize(this, properties);
        }
    }
}