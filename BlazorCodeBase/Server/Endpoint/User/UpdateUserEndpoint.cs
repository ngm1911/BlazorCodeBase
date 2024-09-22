using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Net;
using BlazorCodeBase.Server.Database.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class UpdateUserEndpoint(UserManager<UserInfo> userManager) : Endpoint<UpdateUserRequest>
    {
        public override void Configure()
        {
            Post("/Update");
            Summary(s => {
                s.Summary = "Update";
                s.Description = "This api to update user info";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            UserInfo user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
            {
                await SendOkAsync(Responses.UserNotFound, ct);
            }
            else
            {
                user = new UserInfoBuilder(user).SetFirstName(req.FirstName)
                                         .SetLastName(req.LastName)
                                         .SetEmail(req.Email)
                                         .Build();
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    Response = TypedResults.BadRequest(updateResult.Errors);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(req.Role))
                    {
                        await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));
                        await userManager.AddToRoleAsync(user, req.Role);
                    }
                    var role = new List<string>() { req.Role };
                    var userInfoResponse = new UserInfoResponse(user.FirstName,
                                                       user.FirstName,
                                                       user.Email,
                                                       role);
                    await SendCreatedAtAsync(nameof(GetUserInfoEndpoint), new { user.Email }, userInfoResponse);
                }   
            }
        }
    }

    record UpdateUserRequest(string? FirstName, string? LastName, string? Email, string? Role);

    class UpdateUserValidator : Validator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty();

            RuleFor(x => x.LastName)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }
}
