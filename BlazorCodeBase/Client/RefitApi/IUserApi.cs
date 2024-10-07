using BlazorCodeBase.Shared;
using Refit;

namespace BlazorCodeBase.Client.RefitApi
{
    public interface IUserApi
    {
        [Post("/api/User/Login")]
        Task<HttpResult> LoginAsync([Body] LoginRequest request);

        [Post("/api/User/Logout")]
        Task<HttpResponseMessage> LogoutAsync();

        [Post("/api/User/VerifyTwoFactorCode")]
        Task<HttpResult> Verify2FAAsync([Body] string? Code);

        [Get("/api/User/CurrentUserInfo")]
        Task<HttpResult<UserInfoResponse>> CurrentUserInfoAsync();
        
        [Post("/api/User/Register")]
        Task<HttpResult> RegisterAsync([Body]  RegisterRequest request);

        [Post("/api/SendMail/Register")]
        Task<HttpResult<string>> SendMailRegisterAsync([Body] RegisterMailRequest request);
    }
}
