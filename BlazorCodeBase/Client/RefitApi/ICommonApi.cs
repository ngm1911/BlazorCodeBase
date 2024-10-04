using Refit;

namespace BlazorCodeBase.Client.RefitApi
{
    public interface ICommonApi
    {
        [Get("/api/pingServer")]
        Task<HttpResponseMessage> PingServer();
    }
}
