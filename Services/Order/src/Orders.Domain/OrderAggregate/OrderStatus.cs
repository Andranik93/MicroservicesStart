namespace Orders.Domain.OrderAggregate;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Paid = 4,
    PaymentFailed = 5
}
