using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message, CancellationToken cancellationToken);

        Task SendEmailConfirmationAsync(string email, string link, CancellationToken cancellationToken);
    }
}