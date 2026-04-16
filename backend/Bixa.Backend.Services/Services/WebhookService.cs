using System.Text;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.WebhookModel;
using Bixa.Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bixa.Backend.Services.Services;

public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<string> _webhookUrls;

    public WebhookService(HttpClient httpClient, IConfiguration configuration, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _webhookUrls = _configuration.GetSection("WebhookSettings:NotificationUrls").Get<List<string>>() ?? new List<string>();
        if (!_webhookUrls.Any())
            _logger.LogWarning("No webhook URLs configured in appsettings.json under 'WebhookSettings:NotificationUrls'. Webhook notifications will not be sent.");
    }

    public async Task<Result<bool>> SendWebhookNotificationAsync<T>(WebhookPayload<T> payload) where T : class
    {
        if (!_webhookUrls.Any())
            return Result.Fail<bool>("No webhook URLs configured.", ErrorTypeEnum.General);

        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        bool anySuccess = false;
        foreach (var url in _webhookUrls)
        {
            try
            {
                var currentJsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Sending webhook notification to {Url} for {EntityType} {EntityId} ({ChangeType})",
                    url, payload.EntityType, payload.EntityId, payload.ChangeType);

                var response = await _httpClient.PostAsync(url, currentJsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook notification successfully sent to {Url}", url);
                    anySuccess = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send webhook notification to {Url}. Status: {StatusCode}, Content: {Content}",
                        url, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending webhook notification to {Url}", url);
            }
        }

        if (anySuccess)
        {
            return Result.Success(true);
        }
        else
        {
            return Result.Fail<bool>("Failed to send webhook notification to any configured endpoint.", ErrorTypeEnum.General);
        }
    }
}