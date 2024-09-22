using FastEndpoints;

namespace BlazorCodeBase.Server.Endpoint.User
{
    public class SendMail : Group
    {
        public SendMail()
        {
            Configure("SendMail", ep =>             
            {
                ep.Description(x => x
                  .WithTags("SendMail"));
            });
        }
    }
}
