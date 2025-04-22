using CycleAPI.Models.DTO;
using CycleAPI.Service.Interface;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace CycleAPI.Service.Implementation
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(IConfiguration configuration, IOrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
            _keyId = _configuration["Razorpay:KeyId"];
            _keySecret = _configuration["Razorpay:KeySecret"];
        }

        public async Task<PaymentDto> CreatePaymentOrderAsync(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            var client = new RazorpayClient(_keyId, _keySecret);

            var orderOptions = new Dictionary<string, object>
            {
                { "amount", order.TotalAmount * 100 },  // Amount in smallest currency unit (paise)
                { "currency", "INR" },
                { "receipt", order.OrderNumber }
            };

            Razorpay.Api.Order razorpayOrder = client.Order.Create(orderOptions);

            return new PaymentDto
            {
                OrderId = order.OrderId.ToString(),
                Amount = order.TotalAmount,
                Currency = "INR",
                Receipt = order.OrderNumber,
                RazorpayKey = _keyId,
                RazorpayOrderId = razorpayOrder["id"]
            };
        }

        public async Task<bool> VerifyPaymentAsync(PaymentVerificationDto paymentVerificationDto)
        {
            string generatedSignature = GeneratePaymentSignature(
                paymentVerificationDto.OrderId,
                paymentVerificationDto.PaymentId);

            bool isValid = generatedSignature.Equals(paymentVerificationDto.Signature);
            
            await UpdateOrderPaymentStatusAsync(paymentVerificationDto.OrderId, isValid);
            
            return isValid;
        }

        public async Task UpdateOrderPaymentStatusAsync(string orderId, bool isSuccess)
        {
            if (isSuccess)
            {
                // Update order status to confirmed/paid
                await _orderService.UpdateOrderStatusAsync(
                    Guid.Parse(orderId), 
                    Models.Enums.OrderStatus.PaymentConfirmed);
            }
            else
            {
                // Update order status to payment failed
                await _orderService.UpdateOrderStatusAsync(
                    Guid.Parse(orderId), 
                    Models.Enums.OrderStatus.PaymentFailed);
            }
        }

        private string GeneratePaymentSignature(string orderId, string paymentId)
        {
            string payload = $"{orderId}|{paymentId}";
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}