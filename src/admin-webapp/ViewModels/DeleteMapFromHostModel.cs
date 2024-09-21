namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class DeleteMapFromHostModel
    {
        public DeleteMapFromHostModel()
        {

        }

        public DeleteMapFromHostModel(Guid gameServerId, string mapName)
        {
            GameServerId = gameServerId;
            MapName = mapName;
        }

        public Guid GameServerId { get; set; }
        public string? MapName { get; set; }
    }
}
