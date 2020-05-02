namespace XI.Portal.Maps.Interfaces
{
    public interface IMapFileRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }

        void Validate();
    }
}