using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Demos.Models
{
    public class DemoAuthEntity : TableEntity
    {
        public string AuthKey { get; set; }
    }
}