using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Login : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value = 0;

        private LoginRequest _registerRequest = new();

        public string? TwoFactorCode { get; set; }

        public bool IsLoading { get; set; }

        private async Task DoLogin()
        {
            try
            {
                IsLoading = true;
                var response = await HttpClient.PostAsJsonAsync($"api/User/Login", _registerRequest);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                    if (result?.Errors?.Count > 0)
                    {
                        var errors = result.Errors.SelectMany(x => x.Value);
                        foreach (var error in errors)
                        {
                            ToastService.ShowToast(ToastIntent.Error, error);
                        }
                    }
                    else if (result?.StatusCode != (int)HttpStatusCode.OK)
                    {
                        ToastService.ShowToast(ToastIntent.Error, result.Message);
                    }
                    else
                    {
                        await MyWizard.GoToStepAsync(Value + 1);
                    }
                }
                else
                {
                    ToastService.ShowToast(ToastIntent.Error, content);
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
                var response = await HttpClient.PostAsJsonAsync($"api/User/VerifyTwoFactorCode", new { Code = TwoFactorCode });
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                    if (result?.Errors?.Count > 0)
                    {
                        var errors = result.Errors.SelectMany(x => x.Value);
                        foreach (var error in errors)
                        {
                            ToastService.ShowToast(ToastIntent.Error, error);
                        }
                    }
                    else if (result?.StatusCode != (int)HttpStatusCode.Created)
                    {
                        ToastService.ShowToast(ToastIntent.Error, result.Message);
                    }
                    else
                    {
                        var userVerified = JsonConvert.DeserializeObject<User>(result?.Data.ToString());
                        if (userVerified is not null)
                        {
                            await AuthorizationUserService.PersistUserToBrowser(userVerified)
                                                          .ConfigureAwait(true);
                            NavigationManager.NavigateTo("mainScreen");
                        }
                    }
                }
                else
                {
                    ToastService.ShowToast(ToastIntent.Error, content);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoginGoogle()
        {
            try
            {
                IsLoading = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private class LoginRequest
        {
            [Required]
            public string? UserName { get; set; }

            [Required]
            public string? Password { get; set; }
        }
    }
}