using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Demos.Models
{
    internal class DemoAuthEntity : TableEntity
    {
        public string AuthKey { get; set; }
    }
}