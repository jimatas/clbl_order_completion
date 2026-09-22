namespace OrderCompletion.Api.Unit.Tests.Models;

using OrderCompletion.Api.Models;
using Xunit;

public class OrderTests
{
    private static readonly DateTime EvaluationTimeUtc = new(2026, 9, 22, 9, 0, 0, DateTimeKind.Utc);
    
    [Fact]
    public void GivenFinishedOrder_IsEligibleForCompletion_ReturnsFalse()
    {
        var sut = CreateEligibleOrder();

        sut.OrderState = OrderState.Finished;

        Assert.False(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenOrderWithoutOrderLines_IsEligibleForCompletion_ReturnsFalse()
    {
        var sut = CreateEligibleOrder();

        sut.OrderLines = [];

        Assert.False(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenUnderdeliveredOrder_IsEligibleForCompletion_ReturnsFalse()
    {
        var sut = CreateEligibleOrder();

        sut.OrderLines.Last().DeliveredQuantity = 0;

        Assert.False(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenOverdeliveredOrder_IsEligibleForCompletion_ReturnsFalse()
    {
        var sut = CreateEligibleOrder();

        sut.OrderLines.Last().DeliveredQuantity = 2;

        Assert.False(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenOrderPlacedFiveMonthsAgo_IsEligibleForCompletion_ReturnsFalse()
    {
        var sut = CreateEligibleOrder();

        sut.OrderDate = EvaluationTimeUtc.AddMonths(-5);

        Assert.False(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenOrderPlacedExactlySixMonthsAgo_IsEligibleForCompletion_ReturnsTrue()
    {
        var sut = CreateEligibleOrder();

        Assert.True(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    [Fact]
    public void GivenOrderPlacedMoreThanSixMonthsAgo_IsEligibleForCompletion_ReturnsTrue()
    {
        var sut = CreateEligibleOrder();

        sut.OrderDate = EvaluationTimeUtc.AddMonths(-7);

        Assert.True(sut.IsEligibleForCompletion(EvaluationTimeUtc));
    }

    private static Order CreateEligibleOrder() => new()
    {
        Id = 101,
        OrderDate = EvaluationTimeUtc.AddMonths(-6),
        OrderLines =
        [
            new OrderLine
            {
                Id = 1_001,
                ProductId = 1,
                OrderedQuantity = 1,
                DeliveredQuantity = 1
            },
            new OrderLine
            {
                Id = 1_002,
                ProductId = 2,
                OrderedQuantity = 1,
                DeliveredQuantity = 1
            }
        ],
        OrderState = OrderState.Submitted
    };
}