using FastEndpoints;
using FluentValidation;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BlazorCodeBase.Server.Endpoint.User;
using Microsoft.AspNetCore.Authentication.Google;
using Google.Apis.Auth.OAuth2;
using BlazorCodeBase.Server.Model.Common;
using Microsoft.Extensions.Options;

namespace BlazorCodeBase.Server.Endpoint.Google
{
    class GoogleResponseEndpoint(IOptionsSnapshot<Settings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/signin-google");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "GoogleResponse";
                s.Description = "This api to received reponse from google";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<GoogleGroup>();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result1 = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            string[] scopes = { "https://www.googleapis.com/auth/userinfo.profile", "https://www.googleapis.com/auth/userinfo.email" };
            var result = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
            {
                ClientId = settings.Value.GoogleAuthen.ClientId,
                ClientSecret = settings.Value.GoogleAuthen.ClientSecret,
            }, scopes, "user", ct);

            await SendOkAsync(ct);
        }
    }
}
