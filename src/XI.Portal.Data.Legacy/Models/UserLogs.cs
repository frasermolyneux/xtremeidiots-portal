using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class UserLogs
    {
        public Guid UserLogId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApplicationUserId { get; set; }

        public virtual AspNetUsers ApplicationUser { get; set; }
    }
}