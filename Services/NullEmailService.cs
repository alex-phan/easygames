namespace EasyGames.Services;
// Vaesna file//
public class NullEmailService : IEmailService
{
    // No-op single send (logs to console for demo)
    public Task SendAsync(string to, string subject, string htmlBody)
    {
        Console.WriteLine($"[EMAIL-DEV] To={to} | Subject={subject} | Length={htmlBody?.Length ?? 0}");
        return Task.CompletedTask;
    }

    // No-op bulk send
    public Task SendBulkAsync(IEnumerable<string> to, string subject, string htmlBody)
        => Task.WhenAll(to.Select(addr => SendAsync(addr, subject, htmlBody)));

    // If the code calls this to render an email template, just return a simple HTML
    public Task<string> RenderViewToStringAsync(string viewPath, object model)
        => Task.FromResult("<h3>(DEV) Email body</h3><p>This is a development stub. No email was sent.</p>");
}
