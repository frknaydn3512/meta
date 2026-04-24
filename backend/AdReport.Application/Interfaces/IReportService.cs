using AdReport.Application.DTOs.Report;
using AdReport.Application.Common;

namespace AdReport.Application.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Creates a pending report record and enqueues a Hangfire background job.
    /// </summary>
    Task<ApiResponse<ReportDto>> CreateReportAsync(int agencyId, CreateReportDto request);

    /// <summary>
    /// Returns all reports for the given agency.
    /// </summary>
    Task<ApiResponse<List<ReportDto>>> GetReportsAsync(int agencyId);

    /// <summary>
    /// Returns a single report by ID, scoped to the agency.
    /// </summary>
    Task<ApiResponse<ReportDto>> GetReportByIdAsync(int agencyId, int reportId);

    /// <summary>
    /// Fetches Meta API data, builds ReportDataDto, generates PDF, and marks the report complete.
    /// Called by the Hangfire background job.
    /// </summary>
    Task ProcessReportAsync(int reportId);

    /// <summary>
    /// Returns the full report data for public slug-based access (no auth required).
    /// </summary>
    Task<ApiResponse<ReportDataDto>> GetReportBySlugAsync(string slug);
}
