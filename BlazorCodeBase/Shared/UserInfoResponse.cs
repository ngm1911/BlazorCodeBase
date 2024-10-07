using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCodeBase.Shared
{
    public record UserInfoResponse(string? FirstName, string? LastName, string? Email, string? UserName, IEnumerable<string> Roles);
}
