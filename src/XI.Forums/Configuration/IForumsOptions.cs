namespace XI.Forums.Configuration
{
    public interface IForumsOptions
    {
        string BaseUrl { get; set; }
        string ApiKey { get; set; }

        void Validate();
    }
}