using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace BlazorCodeBase.Client.RefitApi
{
    public class RequestHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var rawResponse = await base.SendAsync(request, ct);

            var content = await rawResponse.Content.ReadAsStringAsync(ct);
            if (rawResponse.IsSuccessStatusCode)
            {
                rawResponse.Content = JsonContent.Create(Array.Empty<string>());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    rawResponse.Content = JsonContent.Create(new string[] { rawResponse.ReasonPhrase });
                }
                else
                {
                    if (TryDeserializeObject(content, out ValidateError[]? validateErrors))
                    {
                        rawResponse.Content = JsonContent.Create(validateErrors.Select(x => x.description));
                    }
                    else if (TryDeserializeObject(content, out ResponseError? responseErrors))
                    {
                        rawResponse.Content = JsonContent.Create(responseErrors.errors.SelectMany(x => x.Value));
                    }
                    else
                    {
                        rawResponse.Content = JsonContent.Create(new string[] { content });
                    }
                }
                rawResponse.StatusCode = HttpStatusCode.OK;
            }
            return rawResponse;
        }


        bool TryDeserializeObject<T>(string json, out T? result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }
    }

    public class BaseResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object? Data { get; set; }

        public int StatusCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string[]>? Errors { get; set; }

        public BaseResponse(object? data = null, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = "OK")
        {
            Data = data;
            StatusCode = (int)statusCode;
            Message = message;
        }
    }

    public class ValidateError
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class ResponseError
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public Dictionary<string, string[]> errors { get; set; }
    }
}