using BlazorCodeBase.Server.Model.Command;
using BlazorCodeBase.Shared;
using FastEndpoints;
using FluentValidation;
using System.Net;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class RegisterEndpoint(RegisterBuilder registerBuilder) : Endpoint<RegisterRequest, UserInfoResponse>
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
            var errors = await registerBuilder.SetUserName(req.UserName)
                                              .SetEmail(req.Email)
                                              .SetLastName(req.LastName)
                                              .SetFirstName(req.FirstName)
                                              .SetPassword(req.Password)
                                              .SetTwoFactorEnabled(true)
                                              .Build()
                                              .ExecuteAsync(ct);
            if (errors.Any())
            {
                await SendResultAsync(TypedResults.BadRequest(errors));
            }
            else
            {
                var userInfoResponse = new UserInfoResponse(req.FirstName,
                                                            req.FirstName,
                                                            req.Email,
                                                            req.UserName,
                                                           ["Guest"]);
                await SendCreatedAtAsync(nameof(GetUserInfoEndpoint), new { req.Email }, userInfoResponse, cancellation: ct);
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

    public record RegisterTemplateModel(string UserName, string? FirstName, string? LastName, string? Url);

}
