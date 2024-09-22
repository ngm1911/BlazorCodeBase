using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Net;
using System.Net.Http;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class RegisterEndpoint(UserManager<UserInfo> userManager, UserInfoBuilder userInfoBuilder, SendMailBuilder mailBuilder) : Endpoint<RegisterRequest, UserInfoResponse>
    {
        public override void Configure()
        {
            Post("/Register");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Register user";
                s.Description = "This api to register new user";
                s.Responses[(int)HttpStatusCode.Created] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
        {
            var user = userInfoBuilder.SetEmail(req.Email)
                                      .SetUserName(req.UserName)
                                      .SetFirstName(req.FirstName)
                                      .SetLastName(req.LastName)
                                      .SetTwoFactorEnabled(true)
                                      .Build();

            var result = await userManager.CreateAsync(user, req.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Guest");
                var userInfoResponse = new UserInfoResponse(user.FirstName,
                                                           user.FirstName,
                                                           user.Email,
                                                           ["Guest"]);
                await SendCreatedAtAsync(nameof(GetUserInfoEndpoint), new { user.Email }, userInfoResponse, cancellation: ct);
            }
            else
            {
                await SendResultAsync(TypedResults.BadRequest(result.Errors));
            }
        }
    }

    class RegisterValidator : Validator<RegisterRequest>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty();

            RuleFor(x => x.FirstName)
                .NotEmpty();

            RuleFor(x => x.LastName)
                .NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }

    record RegisterRequest(string UserName, string? FirstName, string? LastName, string? Password, string? Email);
    public record RegisterTemplateModel(string UserName, string? FirstName, string? LastName, string? Url);
}
