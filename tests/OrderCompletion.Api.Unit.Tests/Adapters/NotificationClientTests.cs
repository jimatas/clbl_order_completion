namespace OrderCompletion.Api.Unit.Tests.Adapters;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderCompletion.Api.Adapters.NotificationAdapter;
using OrderCompletion.Api.Ports;
using System.Net;
using Xunit;

public class NotificationClientTests
{
    [Fact]
    public void GivenTransientFailures_OrderCompleted_RetriesRequest()
    {
        var attempts = 0;

        using var serviceProvider = CreateServiceProvider(_ =>
        {
            attempts++;

            return new(attempts < 3
                ? HttpStatusCode.InternalServerError
                : HttpStatusCode.OK);
        });

        var sut = serviceProvider.GetRequiredService<INotificationClient>();

        sut.OrderCompleted(101);

        Assert.Equal(3, attempts);
    }

    [Fact]
    public void GivenSuccessfulResponse_OrderCompleted_SendsExpectedRequest()
    {
        HttpMethod? method = null;
        Uri? requestUri = null;

        using var serviceProvider = CreateServiceProvider(request =>
        {
            method = request.Method;
            requestUri = request.RequestUri;

            return new(HttpStatusCode.OK);
        });

        var sut = serviceProvider.GetRequiredService<INotificationClient>();

        sut.OrderCompleted(101);

        Assert.Equal(new("http://notification-api/notify/101"), requestUri);
        Assert.Equal(HttpMethod.Get, method);
    }

    [Fact]
    public void GivenPersistentFailure_OrderCompleted_RetriesAndThrows()
    {
        var attempts = 0;

        using var serviceProvider = CreateServiceProvider(_ =>
        {
            attempts++;

            return new(HttpStatusCode.InternalServerError);
        });

        var sut = serviceProvider.GetRequiredService<INotificationClient>();

        var exception = Assert.Throws<NotificationDeliveryException>(() => sut.OrderCompleted(101));

        Assert.Equal(4, attempts); // Initial attempt + 3 retries.
        Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Contains("order 101 could not be delivered", exception.Message);
    }

    private static ServiceProvider CreateServiceProvider(
        Func<HttpRequestMessage, HttpResponseMessage> send)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["NotificationApi:BaseUrl"] = "http://notification-api/",
                ["NotificationApi:MaxRetryAttempts"] = "3"
            })
            .Build();

        var handler = new StubHttpMessageHandler(send);

        var services = new ServiceCollection();

        services.RegisterNotificationAdapter(
            configuration,
            builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));

        return services.BuildServiceProvider();
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override HttpResponseMessage Send(
            HttpRequestMessage request,
            CancellationToken _) => send(request);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken _) => Task.FromResult(send(request));
    }
}