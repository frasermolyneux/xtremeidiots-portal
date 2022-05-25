namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class CollectionResponseDto<T>
    {
        public int Skipped { get; set; }

        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<T> Entries { get; set; } = new List<T>();
    }
}
