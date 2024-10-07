using BlazorCodeBase.Shared;
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
            if (rawResponse.IsSuccessStatusCode == false)
            {
                object httpResult;
                if (string.IsNullOrWhiteSpace(content))
                {
                    httpResult = new HttpResult()
                                        .AddErrors(rawResponse.ReasonPhrase);
                }
                else
                {
                    if (tryDeserializeObject(content, out ValidateError[]? validateErrors))
                    {
                        httpResult = new HttpResult()
                                        .AddErrors(validateErrors.Select(x => x.description).ToArray());
                    }
                    else if (tryDeserializeObject(content, out ResponseError? responseErrors))
                    {
                        httpResult = new HttpResult()
                                        .AddErrors(responseErrors.errors.SelectMany(x => x.Value).ToArray());
                    }
                    else
                    {
                        httpResult = new HttpResult()
                                        .AddErrors(content);
                    }
                }
                rawResponse.Content = JsonContent.Create(httpResult);
                rawResponse.StatusCode = HttpStatusCode.OK;
            }
            return rawResponse;

            bool tryDeserializeObject<T>(string json, out T? result)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(json);
                    return true;
                }
                catch
                {
                    result = default;
                    return false;
                }
            }
        }
    }

    class ValidateError
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    class ResponseError
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public Dictionary<string, string[]> errors { get; set; }
    }
}