namespace XtremeIdiots.Portal.ServersApi.Abstractions.Models
{
    public class ServerRconStatusResponseDto
    {
        public IList<ServerRconPlayerDto> Players { get; set; } = new List<ServerRconPlayerDto>();
    }
}
