using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Register : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value = 0;
        bool IsLoading { get; set; }
        bool IsSending { get; set; } = true;

        CancellationTokenSource token = new();

        private RegisterRequest _registerRequest = new();

        async Task SendRegisterMail()
        {
            try
            {
                await token.CancelAsync();
                token = new CancellationTokenSource();

                IsSending = true;
                var result = await IUserApi.SendMailRegisterAsync(new (_registerRequest.Email));
                if (!result.IsSuccess)
                {
                    foreach (var error in result.Errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                IsSending = false;
            }
        }

        private async Task Submit()
        {
            try
            {
                IsLoading = true;
                var result = await IUserApi.RegisterAsync(_registerRequest);
                if (!result.IsSuccess)
                {
                    foreach (var error in result.Errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
                else
                {
                    IsLoading = false;

                    ToastService.ShowToast(ToastIntent.Success, "OK");
                    await MyWizard.GoToStepAsync(Value + 1);
                    await SendRegisterMail();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
