using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class MapFiles
    {
        public Guid MapFileId { get; set; }
        public string FileName { get; set; }
        public Guid MapMapId { get; set; }

        public virtual Maps MapMap { get; set; }
    }
}