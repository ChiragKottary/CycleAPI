namespace CycleAPI.Models.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        PaymentConfirmed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Returned = 6,
        PaymentFailed = 7,
    }
}