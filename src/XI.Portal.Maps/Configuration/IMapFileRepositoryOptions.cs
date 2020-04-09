namespace XI.Portal.Maps.Configuration
{
    public interface IMapFileRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}