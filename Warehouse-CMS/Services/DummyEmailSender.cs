using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Warehouse_CMS.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"Email would have been sent to: {email}");
            Console.WriteLine($"Subject: {subject}");

            return Task.CompletedTask;
        }
    }
}
