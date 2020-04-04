﻿using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.AzureTableLogging
{
    public class LogEntity : TableEntity
    {
        public string LogLevel { get; set; }
        public string EventId { get; set; }
        public string Message { get; set; }
    }
}