namespace OrderCompletion.Api.Ports;

public class NotificationDeliveryException(string? message, Exception? inner) : Exception(message, inner);
