namespace XI.Portal.Maps.Interfaces
{
    public interface IMapsRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}