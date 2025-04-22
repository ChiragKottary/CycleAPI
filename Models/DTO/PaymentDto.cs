namespace CycleAPI.Models.DTO
{
    public class PaymentDto
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; }
        public string RazorpayKey { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }

    public class PaymentCreateRequestDto
    {
        public Guid OrderId { get; set; }
    }

    public class PaymentVerificationDto
    {
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Signature { get; set; }
    }
}