using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using AdReport.Application.Interfaces;

namespace AdReport.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _smtpHost = configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost not configured");
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUser = configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("Email:SmtpUser not configured");
        _smtpPass = configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("Email:SmtpPassword not configured");
        _fromEmail = configuration["Email:FromEmail"] ?? _smtpUser;
        _fromName = configuration["Email:FromName"] ?? "AdReport";
    }

    /// <inheritdoc/>
    public async Task SendReportEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
