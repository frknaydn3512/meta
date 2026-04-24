using AdReport.Application.DTOs.MetaAccount;
using AdReport.Application.Common;

namespace AdReport.Application.Interfaces;

public interface IMetaAccountService
{
    /// <summary>
    /// Returns all Meta ad accounts connected to the agency.
    /// </summary>
    Task<ApiResponse<List<MetaAccountDto>>> GetAccountsAsync(int agencyId);

    /// <summary>
    /// Validates and connects a new Meta ad account.
    /// </summary>
    Task<ApiResponse<MetaAccountDto>> ConnectAccountAsync(int agencyId, ConnectMetaAccountDto request);

    /// <summary>
    /// Disconnects (removes) a Meta ad account.
    /// </summary>
    Task<ApiResponse<object>> DisconnectAccountAsync(int agencyId, int accountId);

    /// <summary>
    /// Exchanges an OAuth code for a long-lived token and returns the accessible ad accounts.
    /// The token is returned encrypted so the client can echo it back in the confirm step.
    /// </summary>
    Task<ApiResponse<MetaOAuthExchangeResultDto>> ExchangeOAuthCodeAsync(int agencyId, MetaOAuthExchangeDto request);

    /// <summary>
    /// Confirms connection of a specific ad account using the encrypted token from the exchange step.
    /// </summary>
    Task<ApiResponse<MetaAccountDto>> ConfirmOAuthConnectionAsync(int agencyId, MetaOAuthConfirmDto request);
}
