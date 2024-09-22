namespace BlazorCodeBase.Server.Model.Common
{
    public class Settings
    {
        public Jwt? Jwt { get; set; }
        public Connectionstring? ConnectionString { get; set; }
        public Mailsettings? MailSettings { get; set; }
    }

    public class Jwt
    {
        public string? Key { get; set; }
        public int? ExpireTime { get; set; }

        public CookieOptions CookieOpt => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            IsEssential = true
        };
    }

    public class Connectionstring
    {
        public string? DbContext { get; set; }
    }

    public class Mailsettings
    {
        public string SenderMail { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsSecure { get; set; }
        public bool IsAuthen { get; set; }
    }
}