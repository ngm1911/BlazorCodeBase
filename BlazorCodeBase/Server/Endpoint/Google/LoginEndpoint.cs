using FastEndpoints;
using FluentValidation;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using BlazorCodeBase.Server.Model.Common;
using Google.Apis.PeopleService.v1;
using Google.Apis.Util.Store;

namespace BlazorCodeBase.Server.Endpoint.Google
{
    class LoginEndpoint(IOptionsSnapshot<Settings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/Login");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Login";
                s.Description = "This api to login by google";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<GoogleGroup>();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var redirectUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/google/signin-google";
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }
    }
}
