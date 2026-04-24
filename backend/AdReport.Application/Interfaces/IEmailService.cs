namespace AdReport.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends a branded report-ready email to the client.
    /// </summary>
    Task SendReportEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[]? pdfAttachment = null,
        string? pdfFileName = null);
}
