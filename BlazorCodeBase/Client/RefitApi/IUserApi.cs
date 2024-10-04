using Refit;
using static BlazorCodeBase.Client.Pages.Login;
using static BlazorCodeBase.Client.Pages.Register;

namespace BlazorCodeBase.Client.RefitApi
{
    public interface IUserApi
    {
        [Post("/api/User/Login")]
        Task<IEnumerable<string>> LoginAsync([Body] LoginRequest request);

        [Post("/api/User/Logout")]
        Task LogoutAsync();

        [Post("/api/User/VerifyTwoFactorCode")]
        Task<IEnumerable<string>> Verify2FAAsync([Body] string? Code);

        [Get("/api/User/CurrentUserInfo")]
        Task<User> CurrentUserInfoAsync();
        
        [Post("/api/User/Register")]
        Task<IEnumerable<string>> RegisterAsync([Body]  RegisterRequest request);

        [Post("/api/User/SendMailRegister")]
        Task<IEnumerable<string>> SendMailRegisterAsync([Body] string? toEmail);
    }
}
