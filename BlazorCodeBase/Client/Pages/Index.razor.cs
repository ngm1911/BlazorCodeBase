using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace BlazorCodeBase.Client.Pages
{
    public partial class Index : OwningComponentBase
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool IsLoading { get; set; }

        private async Task Login()
        {
            try
            {
                IsLoading = true;
                var response = await HttpClient.PostAsJsonAsync($"api/User/Login", new { UserName, Password });
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BaseResponse>(content);
                    if (result.Errors?.Count > 0)
                    {
                        var errors = result.Errors.SelectMany(x => x.Value);
                        foreach (var error in errors)
                        {
                            ToastService.ShowToast(ToastIntent.Error, error);
                        }
                    }
                    else
                    {
                        ToastService.ShowToast(ToastIntent.Error, result.Message);
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
    }
}