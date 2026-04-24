using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AdReport.Application.Interfaces;
using AdReport.Application.DTOs.MetaAccount;

namespace AdReport.Infrastructure.Services;

public class MetaApiClient : IMetaApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _appId;
    private readonly string _appSecret;

    public MetaApiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _baseUrl = configuration["MetaApi:BaseUrl"] ?? "https://graph.facebook.com/v19.0";
        _appId = configuration["MetaApi:AppId"] ?? string.Empty;
        _appSecret = configuration["MetaApi:AppSecret"] ?? string.Empty;
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, string AccountName, string Currency)> ValidateTokenAsync(
        string accessToken, string accountId)
    {
        var url = $"{_baseUrl}/act_{accountId}?fields=name,currency&access_token={accessToken}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return (false, string.Empty, string.Empty);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out _))
            return (false, string.Empty, string.Empty);

        var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var currency = root.TryGetProperty("currency", out var currProp) ? currProp.GetString() ?? string.Empty : string.Empty;

        return (true, name, currency);
    }

    /// <inheritdoc/>
    public async Task<MetaInsightsDto> GetAccountInsightsAsync(
        string accessToken, string accountId, string dateStart, string dateStop)
    {
        var fields = "spend,impressions,clicks,ctr,cpc,actions,action_values";
        var url = $"{_baseUrl}/act_{accountId}/insights"
            + $"?fields={fields}"
            + $"&time_range={{\"since\":\"{dateStart}\",\"until\":\"{dateStop}\"}}"
            + $"&access_token={accessToken}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var data = doc.RootElement.GetProperty("data");
        if (data.GetArrayLength() == 0)
        {
            return new MetaInsightsDto
            {
                DateStart = dateStart,
                DateStop = dateStop
            };
        }

        var item = data[0];

        var spend = ParseDecimal(item, "spend");
        var impressions = ParseLong(item, "impressions");
        var clicks = ParseLong(item, "clicks");
        var ctr = ParseDecimal(item, "ctr");
        var cpc = ParseDecimal(item, "cpc");

        decimal roas = 0;
        long conversions = 0;

        if (item.TryGetProperty("action_values", out var actionValues))
        {
            foreach (var action in actionValues.EnumerateArray())
            {
                if (action.TryGetProperty("action_type", out var actionType)
                    && actionType.GetString() == "offsite_conversion.fb_pixel_purchase")
                {
                    roas = spend > 0 ? ParseDecimal(action, "value") / spend : 0;
                }
            }
        }

        if (item.TryGetProperty("actions", out var actions))
        {
            foreach (var action in actions.EnumerateArray())
            {
                if (action.TryGetProperty("action_type", out var actionType)
                    && actionType.GetString() == "offsite_conversion.fb_pixel_purchase")
                {
                    conversions = ParseLong(action, "value");
                }
            }
        }

        return new MetaInsightsDto
        {
            Spend = spend,
            Impressions = impressions,
            Clicks = clicks,
            Ctr = ctr,
            Cpc = cpc,
            Roas = roas,
            Conversions = conversions,
            DateStart = dateStart,
            DateStop = dateStop
        };
    }

    /// <inheritdoc/>
    public async Task<List<MetaCampaignDto>> GetCampaignsAsync(
        string accessToken, string accountId, string dateStart, string dateStop)
    {
        var fields = "id,name,status,objective";
        var insightFields = "spend,impressions,clicks,action_values";
        var url = $"{_baseUrl}/act_{accountId}/campaigns"
            + $"?fields={fields},insights.time_range({{\"since\":\"{dateStart}\",\"until\":\"{dateStop}\"}}){{{{insightFields}}}}"
            + $"&access_token={accessToken}";

        // Use a simpler two-step approach: first get campaigns, then get insights
        var campaignUrl = $"{_baseUrl}/act_{accountId}/campaigns?fields={fields}&access_token={accessToken}";
        var response = await _http.GetAsync(campaignUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var campaigns = new List<MetaCampaignDto>();
        var data = doc.RootElement.GetProperty("data");

        foreach (var camp in data.EnumerateArray())
        {
            campaigns.Add(new MetaCampaignDto
            {
                Id = camp.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                Name = camp.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                Status = camp.TryGetProperty("status", out var status) ? status.GetString() ?? string.Empty : string.Empty,
                Objective = camp.TryGetProperty("objective", out var obj) ? obj.GetString() ?? string.Empty : string.Empty,
            });
        }

        return campaigns;
    }

    /// <inheritdoc/>
    public async Task<string> ExchangeCodeForLongLivedTokenAsync(string code, string redirectUri)
    {
        var shortLivedUrl = $"{_baseUrl}/oauth/access_token"
            + $"?client_id={_appId}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&client_secret={_appSecret}"
            + $"&code={code}";

        var response = await _http.GetAsync(shortLivedUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var shortLivedToken = doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("No access token returned from Meta");

        var longLivedUrl = $"{_baseUrl}/oauth/access_token"
            + $"?grant_type=fb_exchange_token"
            + $"&client_id={_appId}"
            + $"&client_secret={_appSecret}"
            + $"&fb_exchange_token={shortLivedToken}";

        var extendResponse = await _http.GetAsync(longLivedUrl);
        extendResponse.EnsureSuccessStatusCode();

        var extendJson = await extendResponse.Content.ReadAsStringAsync();
        using var extendDoc = JsonDocument.Parse(extendJson);
        return extendDoc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("No long-lived token returned from Meta");
    }

    /// <inheritdoc/>
    public async Task<List<MetaAdAccountListDto>> GetUserAdAccountsAsync(string accessToken)
    {
        var url = $"{_baseUrl}/me/adaccounts?fields=name,currency&access_token={accessToken}";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var accounts = new List<MetaAdAccountListDto>();
        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            var rawId = item.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
            accounts.Add(new MetaAdAccountListDto
            {
                Id = rawId.Replace("act_", string.Empty),
                Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                Currency = item.TryGetProperty("currency", out var currProp) ? currProp.GetString() ?? string.Empty : string.Empty,
            });
        }
        return accounts;
    }

    private static decimal ParseDecimal(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();
            if (decimal.TryParse(prop.GetString(), out var val))
                return val;
        }
        return 0;
    }

    private static long ParseLong(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt64();
            if (long.TryParse(prop.GetString(), out var val))
                return val;
        }
        return 0;
    }
}
