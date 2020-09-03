using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api.Services
{
    public class EmailSender
        : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailConfirmationAsync(string email, string link, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}