namespace XI.Portal.Maps.Configuration
{
    public interface IMapsOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}