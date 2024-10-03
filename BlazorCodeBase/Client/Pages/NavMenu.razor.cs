using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace BlazorCodeBase.Client.Pages
{
    public partial class NavMenu : OwningComponentBase
    {
        public bool IsLoading { get; set; }

        private async Task LogoutAsync()
        {
            try
            {
                await HttpClient.PostAsync("/api/User/Logout", null)
                                .ConfigureAwait(false);
            }
            finally
            {
                AuthorizationUserService.ClearTokenUser();
                NavigationManager.NavigateTo("/");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            bool authorized = false;
            try
            {
                var result = await HttpClient.GetAsync($"api/pingServer");
                authorized = result.IsSuccessStatusCode;
            }
            finally
            {
                if (authorized == false)
                {
                    await LogoutAsync();
                }
            }
        }
    }
}
