using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class SystemLogs
    {
        public Guid SystemLogId { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; }
    }
}