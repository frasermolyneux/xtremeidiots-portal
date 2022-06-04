using Newtonsoft.Json;

using System.Net;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class ApiResponseDto
    {
        public ApiResponseDto(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        [JsonProperty]
        public HttpStatusCode StatusCode { get; internal set; }

        [JsonProperty]
        public List<string> Errors { get; internal set; } = new List<string>();

        public bool IsSuccess => StatusCode == HttpStatusCode.OK && !Errors.Any();
        public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
    }

    public class ApiResponseDto<T> : ApiResponseDto
    {
        public ApiResponseDto(HttpStatusCode statusCode) : base(statusCode)
        {
            StatusCode = statusCode;
            Result = default(T);
        }

        public ApiResponseDto(HttpStatusCode statusCode, T result) : base(statusCode)
        {
            StatusCode = statusCode;
            Result = result;
        }

        [JsonProperty]
        public T? Result { get; private set; }

        public new bool IsSuccess => StatusCode == HttpStatusCode.OK && !Errors.Any() && Result != null;
    }
}
