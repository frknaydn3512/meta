using AdReport.Application.DTOs.Report;

namespace AdReport.Application.Interfaces;

public interface IPdfGeneratorService
{
    /// <summary>
    /// Generates a PDF report and returns the absolute file path.
    /// </summary>
    Task<string> GenerateAsync(ReportDataDto data);
}
