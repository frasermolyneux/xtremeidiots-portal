using Newtonsoft.Json;

using System.Net;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class ApiResponseDto
    {
        [JsonConstructor]
        public ApiResponseDto()
        {

        }
        public ApiResponseDto(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public ApiResponseDto(HttpStatusCode statusCode, string error)
        {
            StatusCode = statusCode;
            Errors.Add(error);
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
        [JsonConstructor]
        public ApiResponseDto()
        {

        }

        public ApiResponseDto(HttpStatusCode statusCode) : base(statusCode)
        {
            StatusCode = statusCode;
            Result = default(T);
        }

        public ApiResponseDto(HttpStatusCode statusCode, string error) : base(statusCode, error)
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
