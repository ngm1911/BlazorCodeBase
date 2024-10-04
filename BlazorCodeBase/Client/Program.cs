using BlazorCodeBase.Client;
using BlazorCodeBase.Client.RefitApi;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("BlazorCodeBase.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorCodeBase.ServerAPI"));

builder.Services.AddFluentUIComponents(options =>
{
    options.ValidateClassNames = false;
});


builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthorizationUserService>();
builder.Services.AddScoped<AuthorizationProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthorizationProvider>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<RequestHandler>();

builder.Services.AddRefitClient<IUserApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<RequestHandler>()
       .Services
                .AddRefitClient<IGoogleApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));


await builder.Build().RunAsync();
