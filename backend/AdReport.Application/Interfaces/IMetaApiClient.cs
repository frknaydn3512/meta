using AdReport.Application.DTOs.MetaAccount;

namespace AdReport.Application.Interfaces;

public interface IMetaApiClient
{
    /// <summary>
    /// Validates that the access token is valid and returns the associated ad account info.
    /// </summary>
    Task<(bool IsValid, string AccountName, string Currency)> ValidateTokenAsync(string accessToken, string accountId);

    /// <summary>
    /// Retrieves aggregated insights for an ad account over a date range.
    /// </summary>
    Task<MetaInsightsDto> GetAccountInsightsAsync(string accessToken, string accountId, string dateStart, string dateStop);

    /// <summary>
    /// Retrieves the list of campaigns for an ad account.
    /// </summary>
    Task<List<MetaCampaignDto>> GetCampaignsAsync(string accessToken, string accountId, string dateStart, string dateStop);

    /// <summary>
    /// Exchanges an OAuth authorization code for a long-lived access token.
    /// </summary>
    Task<string> ExchangeCodeForLongLivedTokenAsync(string code, string redirectUri);

    /// <summary>
    /// Returns all ad accounts accessible by the given user access token.
    /// </summary>
    Task<List<MetaAdAccountListDto>> GetUserAdAccountsAsync(string accessToken);
}
