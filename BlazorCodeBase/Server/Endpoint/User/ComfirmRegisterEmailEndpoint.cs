using BlazorCodeBase.Server.Model.Common;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Net;
using BlazorCodeBase.Server.Database.Model;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class ComfirmRegisterEmailEndpoint(UserManager<UserInfo> userManager) : Endpoint<ComfirmRegisterEmailRequest>
    {
        public override void Configure()
        {
            Get("/ComfirmRegisterEmail");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "ComfirmRegisterEmail";
                s.Description = "This api to comfirm register email";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(ComfirmRegisterEmailRequest req, CancellationToken ct)
        {
            var user = await userManager.FindByNameAsync(req.UserName);
            if (user is null)
            {
                await SendOkAsync(Responses.UserNotFound, ct);
            }
            else
            {
                var result = await userManager.ConfirmEmailAsync(user, req.Code);
                if (result.Errors.Any())
                {
                    Response = TypedResults.BadRequest(result.Errors);
                }
                else
                {
                    await SendOkAsync(Responses.OK, ct);
                }
            }
        }
    }

    record ComfirmRegisterEmailRequest (string? Code, string? UserName, string? Email);

    class ComfirmRegisterEmailValidator : Validator<ComfirmRegisterEmailRequest>
    {
        public ComfirmRegisterEmailValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty();

            RuleFor(x => x.UserName)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }
}
