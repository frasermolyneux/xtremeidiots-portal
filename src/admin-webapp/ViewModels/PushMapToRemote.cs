namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class PushMapToRemoteViewModel
    {
        public PushMapToRemoteViewModel()
        {

        }

        public PushMapToRemoteViewModel(Guid gameServerId)
        {
            GameServerId = gameServerId;
        }

        public Guid GameServerId { get; set; }
        public string? MapName { get; set; }
    }
}
