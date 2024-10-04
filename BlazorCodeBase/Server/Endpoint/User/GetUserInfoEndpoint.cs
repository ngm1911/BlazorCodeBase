using BlazorCodeBase.Server.Database.DbContext;
using BlazorCodeBase.Server.Database.Model;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class GetUserInfoEndpoint(UserManager<UserInfo> userManager, SQLiteContext dbContext) : Endpoint<UserInfoRequest>
    {
        public override void Configure()
        {
            Get("/{Email}");
            Roles("Admin");
            Description(x => x.WithName(nameof(GetUserInfoEndpoint)));
            Group<UserGroup>();
            Summary(s => {
                s.Summary = "Get user info";
                s.Description = "This api to get user info";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
        }

        public override async Task HandleAsync(UserInfoRequest req, CancellationToken ct)
        {
            var userInfo = await dbContext.UserRoles
                                    .Join(dbContext.Roles,
                                            u => u.RoleId,
                                            r => r.Id,
                                            (u, r) => new
                                            {
                                                u.UserId,
                                                RoleName = r.Name
                                            })
                                    .ToListAsync(ct);
            if (userInfo.Count == 0)
            {
                await SendNotFoundAsync(ct);
            }
            else
            {
                var list = (await userManager.Users
                                             .ToArrayAsync(ct))
                                             .Select(x => new UserInfoResponse(x.FirstName,
                                                                               x.LastName,
                                                                               x.Email,
                                                                               x.UserName,
                                                                               userInfo.Where(x => x.UserId == x.UserId).Select(x => x.RoleName)));

                await SendOkAsync(list.ToArray(), ct);
            }
        }
    }

    record UserInfoRequest(string? Email);

    record UserInfoResponse(string? FirstName, string? LastName, string? Email, string? UserName, IEnumerable<string> Roles);
}
