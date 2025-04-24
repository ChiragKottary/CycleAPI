namespace CycleAPI.Models.DTO
{
    public class PaymentDto
    {
        public required string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public required string Receipt { get; set; }
        public required string RazorpayKey { get; set; }
        public required string RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
        public required string PaymentStatus { get; set; }
    }

    // DTO for initial payment order creation
    public class PaymentOrderResponseDto
    {
        public required string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public required string Receipt { get; set; }
        public required string RazorpayKey { get; set; }
        public required string RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
    }

    public class PaymentCreateRequestDto
    {
        public Guid OrderId { get; set; }
    }

    public class PaymentVerificationDto
    {
        public required string OrderId { get; set; }
        public required string PaymentId { get; set; }
        public required string Signature { get; set; }
    }
}