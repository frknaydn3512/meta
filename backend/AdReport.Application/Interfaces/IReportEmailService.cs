namespace AdReport.Application.Interfaces;

public interface IReportEmailService
{
    /// <summary>
    /// Sends the report-ready email for a specific completed report.
    /// </summary>
    Task SendReportEmailAsync(int reportId);

    /// <summary>
    /// Monthly recurring job: generates reports for all active Meta accounts
    /// and sends emails to their clients. Runs on the 1st of each month.
    /// </summary>
    Task RunMonthlyReportsAsync();
}
