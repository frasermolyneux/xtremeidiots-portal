using System;
using System.Collections.Generic;
using System.ComponentModel;
using XI.CommonTypes;

namespace XI.Portal.Data.Legacy.Models
{
    public class Maps
    {
        public Maps()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            MapFiles = new HashSet<MapFiles>();
            MapRotations = new HashSet<MapRotations>();
            MapVotes = new HashSet<MapVotes>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public Guid MapId { get; set; }

        [DisplayName("Game")] public GameType GameType { get; set; }

        [DisplayName("Map Name")] public string MapName { get; set; }

        [DisplayName("Map File")] public virtual ICollection<MapFiles> MapFiles { get; set; }

        [DisplayName("Map Rotations")] public virtual ICollection<MapRotations> MapRotations { get; set; }

        [Obsolete("Deprecated Feature")]
        [DisplayName("Map Votes")] public virtual ICollection<MapVotes> MapVotes { get; set; }
    }
}