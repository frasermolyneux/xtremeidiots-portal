using System;
using XI.CommonTypes;

namespace XI.Portal.Servers.Dto
{
    public class LogFileMonitorStateDto
    {
        public Guid FileMonitorId { get; set; }
        public Guid ServerId { get; set; }
        public GameType GameType { get; set; }
        public string ServerTitle { get; set; }
        public string FilePath { get; set; }
        public string FtpHostname { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public long RemoteSize { get; set; }
        public DateTime LastReadAttempt { get; set; }
        public DateTime LastRead { get; set; }
        public int PlayerCount { get; set; }
    }
}