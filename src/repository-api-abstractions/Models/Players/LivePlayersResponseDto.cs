namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class LivePlayersResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<LivePlayerDto> Entries { get; set; } = new List<LivePlayerDto>();
    }
}
