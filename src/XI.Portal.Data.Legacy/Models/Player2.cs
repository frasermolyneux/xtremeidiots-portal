using System;
using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Data.Legacy.Models
{
    public class Player2
    {
        public Player2()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            AdminActions = new HashSet<AdminActions>();
            ChatLogs = new HashSet<ChatLogs>();
            MapVotes = new HashSet<MapVotes>();
            PlayerAlias = new HashSet<PlayerAlias>();
            PlayerIpAddresses = new HashSet<PlayerIpAddresses>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public Guid PlayerId { get; set; }
        public GameType GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public string IpAddress { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }

        public virtual ICollection<AdminActions> AdminActions { get; set; }
        public virtual ICollection<ChatLogs> ChatLogs { get; set; }
        public virtual ICollection<MapVotes> MapVotes { get; set; }
        public virtual ICollection<PlayerAlias> PlayerAlias { get; set; }
        public virtual ICollection<PlayerIpAddresses> PlayerIpAddresses { get; set; }
    }
}