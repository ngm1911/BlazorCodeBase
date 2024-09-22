using BlazorCodeBase.Server.Model.Common;
using FastEndpoints;
using Microsoft.Extensions.Options;
using System.Net;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class LogoutEndpoint(IOptionsSnapshot<Settings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/Logout");
            Summary(s => {
                s.Summary = "Logout";
                s.Description = "This api to Logout";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            HttpContext.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt.CookieOpt);
            await SendOkAsync(ct);
        }
    }
}
