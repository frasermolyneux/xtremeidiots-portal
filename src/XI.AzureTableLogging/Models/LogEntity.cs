using System;
using Microsoft.Azure.Cosmos.Table;

namespace XI.AzureTableLogging.Models
{
    public class LogEntity : TableEntity
    {
        public string LogLevel { get; set; }
        public string EventId { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}