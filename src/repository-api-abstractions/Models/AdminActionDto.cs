namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class AdminActionDto
    {
        public Guid AdminActionId { get; set; }
        public Guid PlayerId { get; set; }
        public string GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }

        public DateTime? Expires { get; set; }
        public int ForumTopicId { get; set; }
        public DateTime Created { get; set; }

        public string AdminId { get; set; }
        public string AdminName { get; set; }
    }
}
