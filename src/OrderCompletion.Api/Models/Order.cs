namespace OrderCompletion.Api.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderState OrderState { get; set; }
    public IReadOnlyCollection<OrderLine> OrderLines { get; set; } = Array.Empty<OrderLine>();

    public bool IsEligibleForCompletion(DateTime asOfUtc) =>
        OrderState == OrderState.Submitted &&
        OrderLines.Count > 0 &&
        OrderLines.All(line =>
            line.DeliveredQuantity == line.OrderedQuantity) &&
        OrderDate <= asOfUtc.AddMonths(-6);
}