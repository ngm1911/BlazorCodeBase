using Newtonsoft.Json;
using System.Net;

namespace BlazorCodeBase.Shared
{
    public class BaseResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object? Data {  get; set; }

        public int StatusCode {  get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Message {  get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string[]>? Errors {  get; set; }

        public BaseResponse(object? data = null, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = "OK")
        {
            Data = data;
            StatusCode = (int)statusCode;
            Message = message;
        }
    }
}
