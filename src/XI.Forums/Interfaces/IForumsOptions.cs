namespace XI.Forums.Interfaces
{
    public interface IForumsOptions
    {
        string BaseUrl { get; set; }
        string ApiKey { get; set; }

        void Validate();
    }
}