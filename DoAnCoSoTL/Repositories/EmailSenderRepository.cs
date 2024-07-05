using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailOptions)
    {
        _emailSettings = emailOptions.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Execute(_emailSettings.SendGridApiKey, subject, htmlMessage, email);
    }

    private Task Execute(string apiKey, string subject, string message, string email)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(email));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);

        return client.SendEmailAsync(msg);
    }
}

public class EmailSettings
{
    public string SendGridApiKey { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string SenderEmail { get; internal set; }
    public string? SenderName { get; internal set; }
    public string? SmtpServer { get; internal set; }
    public int SmtpPort { get; internal set; }
    public string? Username { get; internal set; }
    public SecureString? Password { get; internal set; }
}
