using Microsoft.EntityFrameworkCore;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.MetaAccount;
using AdReport.Application.Common;
using AdReport.Domain.Entities;
using AdReport.Infrastructure.Data;

namespace AdReport.Infrastructure.Services;

public class MetaAccountService : IMetaAccountService
{
    private readonly AppDbContext _context;
    private readonly IMetaApiClient _metaApiClient;
    private readonly IEncryptionService _encryption;

    public MetaAccountService(AppDbContext context, IMetaApiClient metaApiClient, IEncryptionService encryption)
    {
        _context = context;
        _metaApiClient = metaApiClient;
        _encryption = encryption;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<MetaAccountDto>>> GetAccountsAsync(int agencyId)
    {
        var accounts = await _context.MetaAccounts
            .Where(a => a.AgencyId == agencyId)
            .Select(a => new MetaAccountDto
            {
                Id = a.Id,
                AgencyId = a.AgencyId,
                ClientId = a.ClientId,
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                Currency = a.Currency,
                CreatedAt = a.CreatedAt
            })
            .OrderBy(a => a.AccountName)
            .ToListAsync();

        return ApiResponse<List<MetaAccountDto>>.SuccessResult(accounts);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<MetaAccountDto>> ConnectAccountAsync(int agencyId, ConnectMetaAccountDto request)
    {
        // Verify the client belongs to this agency
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == request.ClientId);

        if (client == null)
            return ApiResponse<MetaAccountDto>.ErrorResult("Client not found");

        // Prevent duplicate account connections
        var alreadyConnected = await _context.MetaAccounts
            .AnyAsync(a => a.AgencyId == agencyId && a.AccountId == request.AccountId);

        if (alreadyConnected)
            return ApiResponse<MetaAccountDto>.ErrorResult("This Meta account is already connected");

        // Validate the token against the Meta API
        var (isValid, accountName, currency) = await _metaApiClient.ValidateTokenAsync(
            request.AccessToken, request.AccountId);

        if (!isValid)
            return ApiResponse<MetaAccountDto>.ErrorResult("Invalid Meta access token or account ID");

        var account = new MetaAccount
        {
            AgencyId = agencyId,
            ClientId = request.ClientId,
            AccountId = request.AccountId,
            EncryptedAccessToken = _encryption.Encrypt(request.AccessToken),
            AccountName = accountName,
            Currency = currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.MetaAccounts.Add(account);
        await _context.SaveChangesAsync();

        var dto = new MetaAccountDto
        {
            Id = account.Id,
            AgencyId = account.AgencyId,
            ClientId = account.ClientId,
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            Currency = account.Currency,
            CreatedAt = account.CreatedAt
        };

        return ApiResponse<MetaAccountDto>.SuccessResult(dto, "Meta account connected successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<object>> DisconnectAccountAsync(int agencyId, int accountId)
    {
        var account = await _context.MetaAccounts
            .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.Id == accountId);

        if (account == null)
            return ApiResponse<object>.ErrorResult("Meta account not found");

        _context.MetaAccounts.Remove(account);
        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResult(new { }, "Meta account disconnected successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<MetaOAuthExchangeResultDto>> ExchangeOAuthCodeAsync(int agencyId, MetaOAuthExchangeDto request)
    {
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == request.ClientId);

        if (client == null)
            return ApiResponse<MetaOAuthExchangeResultDto>.ErrorResult("Client not found");

        var longLivedToken = await _metaApiClient.ExchangeCodeForLongLivedTokenAsync(request.Code, request.RedirectUri);
        var adAccounts = await _metaApiClient.GetUserAdAccountsAsync(longLivedToken);

        return ApiResponse<MetaOAuthExchangeResultDto>.SuccessResult(new MetaOAuthExchangeResultDto
        {
            EncryptedToken = _encryption.Encrypt(longLivedToken),
            AdAccounts = adAccounts
        });
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<MetaAccountDto>> ConfirmOAuthConnectionAsync(int agencyId, MetaOAuthConfirmDto request)
    {
        var client = await _context.AgencyClients
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Id == request.ClientId);

        if (client == null)
            return ApiResponse<MetaAccountDto>.ErrorResult("Client not found");

        var alreadyConnected = await _context.MetaAccounts
            .AnyAsync(a => a.AgencyId == agencyId && a.AccountId == request.AccountId);

        if (alreadyConnected)
            return ApiResponse<MetaAccountDto>.ErrorResult("This Meta account is already connected");

        var plainToken = _encryption.Decrypt(request.EncryptedToken);
        var (isValid, accountName, currency) = await _metaApiClient.ValidateTokenAsync(plainToken, request.AccountId);

        if (!isValid)
            return ApiResponse<MetaAccountDto>.ErrorResult("Invalid Meta access token or account ID");

        var account = new MetaAccount
        {
            AgencyId = agencyId,
            ClientId = request.ClientId,
            AccountId = request.AccountId,
            EncryptedAccessToken = request.EncryptedToken,
            AccountName = accountName,
            Currency = currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.MetaAccounts.Add(account);
        await _context.SaveChangesAsync();

        return ApiResponse<MetaAccountDto>.SuccessResult(new MetaAccountDto
        {
            Id = account.Id,
            AgencyId = account.AgencyId,
            ClientId = account.ClientId,
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            Currency = account.Currency,
            CreatedAt = account.CreatedAt
        }, "Meta account connected successfully");
    }
}
