using BlazorCodeBase.Client.RefitApi;
using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Login : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value;

        private LoginRequest _registerRequest = new();

        public string? TwoFactorCode { get; set; }

        public bool IsLoading { get; set; }

        private void DoLoginByGoogle()
        {
            NavigationManager.NavigateTo("api/google/login/", true);
        }

        private async Task DoLogin()
        {
            try
            {
                IsLoading = true;
                var result = await IUserApi.LoginAsync(_registerRequest);
                if (!result.IsSuccess)
                {
                    foreach (var error in result.Errors)
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
                var result = await IUserApi.Verify2FAAsync(TwoFactorCode);
                if (!result.IsSuccess)
                {
                    foreach (var error in result.Errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
                else
                {
                    var userInfo = await IUserApi.CurrentUserInfoAsync();
                    if (userInfo.IsSuccess)
                    {
                        await AuthorizationUserService.PersistUserToBrowser(userInfo.Data)
                                                      .ConfigureAwait(true);
                        NavigationManager.NavigateTo("/");
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}