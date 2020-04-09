namespace XI.Portal.Maps.Configuration
{
    public interface IMapsRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}