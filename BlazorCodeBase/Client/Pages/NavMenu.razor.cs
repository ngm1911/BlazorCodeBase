using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace BlazorCodeBase.Client.Pages
{
    public partial class NavMenu : OwningComponentBase
    {
        public bool IsLoading { get; set; }

        private async void Logout()
        {
            await HttpClient.PostAsync("/api/User/Logout", null)
                            .ConfigureAwait(false);
            AuthorizationUserService.ClearTokenUser();
            NavigationManager.NavigateTo("/");
        }
    }
}
