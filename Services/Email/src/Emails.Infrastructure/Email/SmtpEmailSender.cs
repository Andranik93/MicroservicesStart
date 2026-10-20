using Emails.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Emails.Infrastructure.Email;

/// <summary>
/// Sends real SMTP messages - to Mailpit in this project, a fake mail server
/// with a web inbox (http://localhost:8025). Nothing it sends leaves this
/// machine. Swapping Mailpit for a real provider later needs no code change
/// here, only different Smtp:* configuration.
/// </summary>
public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtp = configuration.GetSection("Smtp");
        var host = smtp["Host"] ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        var port = int.Parse(smtp["Port"] ?? "1025");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("no-reply@example.test"));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
