namespace XI.Portal.Maps.Interfaces
{
    public interface IMapRedirectRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }
        string ApiKey { get; set; }

        void Validate();
    }
}
