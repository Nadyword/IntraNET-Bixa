using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.WebhookModel;

namespace Bixa.Backend.Services.Interfaces;

public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification to registered endpoints.
    /// </summary>
    /// <typeparam name="T">The type of the payload (DTO of the changed entity).</typeparam>
    /// <param name="payload">The data to send in the webhook.</param>
    /// <returns>A Result indicating if the notification was sent successfully (at least to one endpoint).</returns>
    Task<Result<bool>> SendWebhookNotificationAsync<T>(WebhookPayload<T> payload) where T : class;
}