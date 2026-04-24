namespace AdReport.Application.Common;

public static class EmailTemplates
{
    /// <summary>
    /// Generates a branded HTML email with a "View Report" CTA button.
    /// </summary>
    public static string ReportReady(
        string clientName,
        string agencyName,
        string? agencyLogoUrl,
        string primaryColor,
        string reportPeriod,
        string reportUrl)
    {
        var logoHtml = string.IsNullOrEmpty(agencyLogoUrl)
            ? $"<span style=\"font-size:22px;font-weight:700;color:{primaryColor}\">{agencyName}</span>"
            : $"<img src=\"{agencyLogoUrl}\" alt=\"{agencyName}\" style=\"max-height:50px;max-width:200px;\" />";

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Your Ads Report is Ready</title>
            </head>
            <body style="margin:0;padding:0;background:#f4f4f7;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f7;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08);">

                      <!-- Header -->
                      <tr>
                        <td style="background:{primaryColor};padding:28px 40px;">
                          {logoHtml}
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:40px;">
                          <h2 style="margin:0 0 12px;color:#111827;font-size:20px;">
                            {reportPeriod} Reklam Raporunuz Hazır
                          </h2>
                          <p style="margin:0 0 24px;color:#6b7280;font-size:15px;line-height:1.6;">
                            Merhaba {clientName},<br/><br/>
                            <strong>{reportPeriod}</strong> dönemine ait Meta Ads performans raporunuz
                            hazırlandı. Raporunuzu görüntülemek için aşağıdaki butona tıklayın.
                          </p>

                          <!-- CTA Button -->
                          <table cellpadding="0" cellspacing="0">
                            <tr>
                              <td style="border-radius:6px;background:{primaryColor};">
                                <a href="{reportUrl}"
                                   style="display:inline-block;padding:14px 32px;color:#ffffff;font-size:15px;font-weight:600;text-decoration:none;border-radius:6px;">
                                  Raporu Görüntüle
                                </a>
                              </td>
                            </tr>
                          </table>

                          <p style="margin:24px 0 0;color:#9ca3af;font-size:13px;">
                            Ya da bu bağlantıyı tarayıcınıza kopyalayın:<br/>
                            <a href="{reportUrl}" style="color:{primaryColor};word-break:break-all;">{reportUrl}</a>
                          </p>
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="padding:20px 40px;border-top:1px solid #e5e7eb;text-align:center;">
                          <p style="margin:0;color:#9ca3af;font-size:12px;">
                            {agencyName} tarafından gönderildi
                          </p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
