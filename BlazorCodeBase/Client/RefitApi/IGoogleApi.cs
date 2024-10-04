using Refit;

namespace BlazorCodeBase.Client.RefitApi
{
    public interface IGoogleApi
    {
        [Get("api/google/login")]
        Task LoginByGoogleAsync();
    }
}
