namespace EasyGames.Services;

using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

//Vaesna file //

public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _opt;
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public SmtpEmailService(
        IOptions<SmtpOptions> opt,
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _opt = opt.Value;
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_opt.Host))
            throw new InvalidOperationException("SMTP not configured: set Smtp:Host (and related fields) in appsettings.");

        // Ensure modern TLS during the SMTP handshake
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        using var client = new SmtpClient(_opt.Host, _opt.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false, // IMPORTANT: always use explicit creds
            EnableSsl = _opt.UseSsl,       // STARTTLS on ports like 587/2525
            Credentials = new NetworkCredential(_opt.Username, _opt.Password),
            Timeout = 15000
        };

        var msg = new MailMessage
        {
            From = new MailAddress(_opt.FromEmail, _opt.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        try
        {
            await client.SendMailAsync(msg);
        }
        catch (SmtpException ex)
        {
            // Surface a clearer error so we know whether it's Host/Port/SSL/Auth
            throw new InvalidOperationException(
                $"SMTP send failed. Check Host/Port/UseSsl/Username/Password/FromEmail. Server said: {ex.Message}", ex);
        }
    }

    public async Task SendBulkAsync(System.Collections.Generic.IEnumerable<string> to, string subject, string htmlBody)
    {
        foreach (var address in to)
        {
            if (!string.IsNullOrWhiteSpace(address))
                await SendAsync(address, subject, htmlBody);
        }
    }

    // Render a Razor view to HTML.
    public async Task<string> RenderViewToStringAsync(string viewPath, object model)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ActionDescriptor()
        );

        using var sw = new StringWriter();

        var viewResult = _viewEngine.GetView(
            executingFilePath: null,
            viewPath: viewPath,
            isMainPage: true
        );

        if (!viewResult.Success)
            throw new InvalidOperationException($"Email view not found: {viewPath}");

        var viewData = new ViewDataDictionary(
            metadataProvider: new EmptyModelMetadataProvider(),
            modelState: new ModelStateDictionary()
        )
        {
            Model = model
        };

        var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}