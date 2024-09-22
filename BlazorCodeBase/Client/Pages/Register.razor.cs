using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Register : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value = 0;

        public bool IsLoading { get; set; }

        private RegisterRequest _registerRequest = new RegisterRequest();

        async Task SendRegisterMail()
        {
            try
            {
                IsLoading = true;
                var t = await HttpClient.PostAsJsonAsync<RegisterMailRequest>($"api/SendMail/Register", new(_registerRequest.Email));
                var content = await t.Content.ReadAsStringAsync();
                if (t.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                    if (result.StatusCode != (int)HttpStatusCode.OK)
                    {
                        var errors = result.Errors.SelectMany(x => x.Value);
                        foreach (var error in errors)
                        {
                            ToastService.ShowToast(ToastIntent.Error, error);
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

        private async Task Submit()
        {
            try
            {
                IsLoading = true;
                var response = await HttpClient.PostAsJsonAsync($"api/User/Register", _registerRequest);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                    if (result.StatusCode == (int)HttpStatusCode.Created)
                    {
                        ToastService.ShowToast(ToastIntent.Success, "OK");
                        await MyWizard.GoToStepAsync(Value + 1);
                        SendRegisterMail();
                    }
                    else
                    {
                        var errors = result.Errors.SelectMany(x => x.Value);
                        foreach (var error in errors)
                        {
                            ToastService.ShowToast(ToastIntent.Error, error);
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

        private class RegisterRequest
        {
            [Required]
            public string? UserName { get; set; }

            [Required]
            public string? FirstName { get; set; }

            [Required]
            public string? LastName { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public string? Password { get; set; }
        }
        record RegisterMailRequest(string? ToEmail);
    }
}
