using OrderCompletion.Api.Ports;
using Polly;
using Polly.Retry;
using System.Net;

namespace OrderCompletion.Api.Adapters.NotificationAdapter;

public static class NotificationAdapter
{
    public static void RegisterNotificationAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<INotificationClient, NotificationClient>();
        services
            .AddHttpClient<INotificationClient, NotificationClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("NotificationApi:BaseUrl")!);
            })
            .AddResilienceHandler("notification-retry", pipelineBuilder =>
            {
                pipelineBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(response =>
                            response.StatusCode == HttpStatusCode.InternalServerError),

                    MaxRetryAttempts = configuration.GetValue<int>("NotificationApi:MaxRetryAttempts")
                });
            });
    }
}