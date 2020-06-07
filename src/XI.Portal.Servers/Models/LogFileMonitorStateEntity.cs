using System;
using XI.AzureTableExtensions;
using XI.AzureTableExtensions.Attributes;
using XI.CommonTypes;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Models
{
    public class LogFileMonitorStateEntity : TableEntityExtended
    {
        // ReSharper disable once UnusedMember.Global - required for ExecuteQuerySegmentedAsync
        public LogFileMonitorStateEntity()
        {
            
        }

        public LogFileMonitorStateEntity(LogFileMonitorStateDto model)
        {
            RowKey = model.FileMonitorId.ToString();
            PartitionKey = "state";

            ServerId = model.ServerId;
            GameType = model.GameType;
            ServerTitle = model.ServerTitle;
            FilePath = model.FilePath;
            FtpHostname = model.FtpHostname;
            FtpUsername = model.FtpUsername;
            FtpPassword = model.FtpPassword;
            RemoteSize = model.RemoteSize;
            LastReadAttempt = model.LastReadAttempt;
            LastRead = model.LastRead;
        }

        public Guid ServerId { get; set; }
        [EntityEnumPropertyConverter] public GameType GameType { get; set; }
        public string ServerTitle { get; set; }
        public string FilePath { get; set; }
        public string FtpHostname { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public long RemoteSize { get; set; }
        public DateTime LastReadAttempt { get; set; }
        public DateTime LastRead { get; set; }
    }
}