namespace XI.Portal.Maps.Interfaces
{
    public interface ILegacyMapsRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}