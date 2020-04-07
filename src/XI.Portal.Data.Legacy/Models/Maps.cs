using System;
using System.Collections.Generic;

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
        public int GameType { get; set; }
        public string MapName { get; set; }

        public virtual ICollection<MapFiles> MapFiles { get; set; }
        public virtual ICollection<MapRotations> MapRotations { get; set; }
        public virtual ICollection<MapVotes> MapVotes { get; set; }
    }
}