using OrderCompletion.Api.Ports;

namespace OrderCompletion.Api.Adapters.NotificationAdapter;

public sealed class NotificationClient(HttpClient httpClient) : INotificationClient
{
    public void OrderCompleted(int orderId)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"notify/{orderId}");
            using var response = httpClient.Send(request);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            throw new NotificationDeliveryException(
                $"Completion notification for order {orderId} could not be delivered.",
                exception);
        }
    }
}