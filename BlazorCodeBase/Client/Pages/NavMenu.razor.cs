using BlazorCodeBase.Client.RefitApi;
using Microsoft.AspNetCore.Components;

namespace BlazorCodeBase.Client.Pages
{
    public partial class NavMenu : OwningComponentBase
    {
        #region Inject
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AuthorizationUserService AuthorizationUserService { get; set; }

        [Inject]
        public ICommonApi ICommonApi { get; set; }

        [Inject]
        public IUserApi IUserApi { get; set; }
        #endregion

        public bool IsLoading { get; set; }

        private async Task LogoutAsync()
        {
            try
            {
                await IUserApi.LogoutAsync()
                              .ConfigureAwait(true);
            }
            finally
            {
                AuthorizationUserService.ClearTokenUser();
                NavigationManager.Refresh(true);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                bool authorized = false;
                try
                {
                    var result = await ICommonApi.PingServer();
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
}
