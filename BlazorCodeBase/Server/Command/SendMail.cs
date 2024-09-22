using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace BlazorCodeBase.Server.Handler
{
    public class SendMailCommand : ICommand<SendResponse>
    {
        public string? FromEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? TemplatePath { get; set; }
        public object Modal { get; set; }
    }

    public class SendMailHandler(IFluentEmail fluentEmail) : ICommandHandler<SendMailCommand, SendResponse>
    {
        public async Task<SendResponse> ExecuteAsync(SendMailCommand command, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(command.TemplatePath) == false)
            {
                var email = await fluentEmail
                            .UsingTemplateFromFile(command.TemplatePath, command.Modal)
                            .To(command.ToEmail)
                            .Subject(command.Subject)
                            .SendAsync(ct);
                return email;
            }
            else
            {
                var email = await fluentEmail
                            .To(command.ToEmail)
                            .Subject(command.Subject)
                            .Body(command.Body)
                            .SendAsync(ct);
                return email;
            }
        }
    }
}
