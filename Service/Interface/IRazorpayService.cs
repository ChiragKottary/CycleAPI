using CycleAPI.Models.DTO;

namespace CycleAPI.Service.Interface
{
    public interface IRazorpayService
    {
        Task<PaymentOrderResponseDto> CreatePaymentOrderAsync(Guid orderId);
        Task<bool> VerifyPaymentAsync(PaymentVerificationDto paymentVerificationDto);
        Task UpdateOrderPaymentStatusAsync(string orderId, bool isSuccess);
    }
}