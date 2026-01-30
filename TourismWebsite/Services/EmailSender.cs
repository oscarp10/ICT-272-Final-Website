using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace TourismWebsite.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Dummy: do nothing
            return Task.CompletedTask;
        }
    }
}
