using BlazorCodeBase.Server.Database.Interface;
using BlazorCodeBase.Server.Handler;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;

namespace BlazorCodeBase.Server.Database.Model
{
    public class UserInfo : IdentityUser, IModified
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? UserModified { get; set; }

        public DateTime? DateModified { get; set; }
    }

    public class UserInfoBuilder
    {
        private readonly UserInfo _userInfo;

        public UserInfoBuilder SetUserName(string userName)
        {
            _userInfo.UserName = userName;
            return this;
        }
        
        public UserInfoBuilder SetEmail(string email)
        {
            _userInfo.Email = email;
            return this;
        }

        public UserInfoBuilder SetFirstName(string firstName)
        {
            _userInfo.FirstName = firstName;
            return this;
        }

        public UserInfoBuilder SetLastName(string lastName)
        {
            _userInfo.LastName = lastName;
            return this;
        }
        
        public UserInfoBuilder SetTwoFactorEnabled(bool enable)
        {
            _userInfo.TwoFactorEnabled = enable;
            return this;
        }

        public UserInfo Build()
        {
            return _userInfo;
        }

        public UserInfoBuilder(UserInfo? userInfo = null)
        {
            _userInfo = userInfo ?? new();
        }
    }
}
