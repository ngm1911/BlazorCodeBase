using BlazorCodeBase.Client.RefitApi;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Login : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value;

        private LoginRequest _registerRequest = new();

        public string? TwoFactorCode { get; set; }

        public bool IsLoading { get; set; }

        public string LoginByGoogle
        {
            get
            {
                return $"{HttpClient.BaseAddress}/api/google/login";
            }
        }

        private async Task DoLogin()
        {
            try
            {
                IsLoading = true;
                var errors = await IUserApi.LoginAsync(_registerRequest);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
                else
                {
                    await MyWizard.GoToStepAsync(Value + 1);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task VerifyCode()
        {
            try
            {
                IsLoading = true;
                var errors = await IUserApi.Verify2FAAsync(TwoFactorCode);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
                else
                {
                    var userVerified = await IUserApi.CurrentUserInfoAsync();
                    if (userVerified != null)
                    {
                        await AuthorizationUserService.PersistUserToBrowser(userVerified)
                                                      .ConfigureAwait(true);
                        NavigationManager.NavigateTo("mainScreen");
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public class LoginRequest
        {
            [Required]
            public string? UserName { get; set; }

            [Required]
            public string? Password { get; set; }
        }
    }
}