using BlazorCodeBase.Shared;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BlazorCodeBase.Client
{
    public class AuthorizationProvider(AuthorizationUserService _authorizationUserService) : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();
            var user = await _authorizationUserService.FetchUserFromBrowser();
            if (user is not null)
            {
                principal = new(new ClaimsIdentity(
                [
                    new (ClaimTypes.Name, user.UserName),
                    new (ClaimTypes.Email, user.Email),
                    new (nameof(user.FirstName), user.FirstName),
                    new (nameof(user.LastName), user.LastName),
                    new (ClaimTypes.Role, string.Join(",", user.Roles))
                ], "BlazorCodeBase"));
            }
            return new(principal);
        }
    }

    public class AuthorizationUserService(ILocalStorageService _protectedLocalStorage, HttpClient _httpClient)
    {
        private readonly string _userStorage = "userStorage";

        public async void ClearBrowserUserData() => await _protectedLocalStorage.ClearAsync();

        public async void ClearTokenUser() => await _protectedLocalStorage.RemoveItemAsync(_userStorage);

        public async Task PersistUserToBrowser(User token)
        {
            await _protectedLocalStorage.SetItemAsync(_userStorage, token);
        }

        public async Task<User?> FetchUserFromBrowser()
        {
            User? user = default!;
            var storage = await _protectedLocalStorage.GetItemAsStringAsync(_userStorage);
            try
            {
                user = JsonConvert.DeserializeObject<User>(storage ?? string.Empty);
            }
            finally
            {
                if (user is null)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"api/User/CurrentUserInfo");
                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<BaseResponse>(await response.Content.ReadAsStringAsync());
                            var userVerified = JsonConvert.DeserializeObject<User>(result?.Data.ToString());
                            user = userVerified;
                            await PersistUserToBrowser(user);
                        }
                    }
                    catch { }
                }
            }
            return user;
        }
    }

    public record User(string? FirstName, string? LastName, string? Email, string? UserName, IEnumerable<string> Roles);
}
