using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyGames.Services
{
    /// Implementations: SmtpEmailService (real) and NullEmailService (stub).
    public interface IEmailService
    {
        /// Sends a single HTML email.
        Task SendAsync(string to, string subject, string htmlBody);

        /// Sends the same HTML email to many recipients.
        Task SendBulkAsync(IEnumerable<string> to, string subject, string htmlBody);

        
        /// Renders a Razor view to an HTML string.
        Task<string> RenderViewToStringAsync(string viewPath, object model);
    }
}
