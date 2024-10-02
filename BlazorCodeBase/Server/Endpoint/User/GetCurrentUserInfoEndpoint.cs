using BlazorCodeBase.Server.Database.DbContext;
using BlazorCodeBase.Server.Database.Model;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class GetCurrentUserInfoEndpoint(UserManager<UserInfo> userManager, 
                                     SQLiteContext dbContext) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/CurrentUserInfo");
            Group<UserGroup>();
            Summary(s => {
                s.Summary = "Get current user info";
                s.Description = "This api to get current user info";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string? email = HttpContext.User.ClaimValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email) == false)
            {
                var user = await userManager.FindByEmailAsync(email);

                var roles = await dbContext.UserRoles
                                    .Join(dbContext.Roles,
                                            u => u.RoleId,
                                            r => r.Id,
                                            (u, r) => new
                                            {
                                                u.UserId,
                                                RoleName = r.Name
                                            })
                                    .Where(x => x.UserId == user.Id)
                                    .Select(x => x.RoleName)
                                    .ToListAsync(ct);

                var result = new UserInfoResponse(user.FirstName,
                                                  user.LastName,
                                                  user.Email,
                                                  user.UserName,
                                                  roles);

                await SendOkAsync(result, ct);
            }
            else
            {
                await SendOkAsync(Responses.UnAuthorized, ct);
            }
        }
    }
}
