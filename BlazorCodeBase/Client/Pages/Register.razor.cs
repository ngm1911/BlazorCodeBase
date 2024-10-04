using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Register : OwningComponentBase
    {
        FluentWizard MyWizard = default!;
        int Value = 0;

        public bool IsLoading { get; set; }

        private RegisterRequest _registerRequest = new();

        async Task SendRegisterMail()
        {
            try
            {
                IsLoading = true;
                var errors = await IUserApi.SendMailRegisterAsync(_registerRequest.Email);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
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
                var errors = await IUserApi.RegisterAsync(_registerRequest);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ToastService.ShowToast(ToastIntent.Error, error);
                    }
                }
                else
                {
                    ToastService.ShowToast(ToastIntent.Success, "OK");
                    await MyWizard.GoToStepAsync(Value + 1);
                    SendRegisterMail();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public class RegisterRequest
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
