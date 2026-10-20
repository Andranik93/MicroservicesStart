namespace Emails.Application.Abstractions;

/// <summary>
/// Sends an email over SMTP. Defined here (dependency inversion), implemented
/// in Infrastructure with MailKit, pointed at Mailpit for this project.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
