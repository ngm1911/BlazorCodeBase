using BlazorCodeBase.Server.Handler;

namespace BlazorCodeBase.Server.Model.Command
{
    public class SendMailBuilder
    {
        private readonly SendMailCommand sendMail = new();

        public SendMailBuilder SetFrom(string fromEmail)
        {
            sendMail.FromEmail = fromEmail;
            return this;
        }

        public SendMailBuilder SetTo(string toEmail)
        {
            sendMail.ToEmail = toEmail;
            return this;
        }

        public SendMailBuilder SetSubject(string subject)
        {
            sendMail.Subject = subject;
            return this;
        }

        public SendMailBuilder SetBody(string body)
        {
            sendMail.Body = body;
            return this;
        }
        
        public SendMailBuilder SetUsingTemplate(string templatePath)
        {
            sendMail.TemplatePath = templatePath;
            return this;
        }
        
        public SendMailBuilder SetModal(object modal)
        {
            sendMail.Modal = modal;
            return this;
        }

        public SendMailCommand Build()
        {
            return sendMail;
        }
    }
}
