using CycleAPI.Models.DTO;
using CycleAPI.Models.Enums;
using CycleAPI.Service.Interface;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using CycleAPI.Repositories.Interface;

namespace CycleAPI.Service.Implementation
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(
            IConfiguration configuration, 
            IOrderService orderService,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
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

            // Create payment record
            var payment = new Models.Domain.Payment
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                RazorpayOrderId = razorpayOrder["id"],
                Amount = order.TotalAmount,
                Currency = "INR",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.CreateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

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
            
            // Update payment record
            var payment = await _unitOfWork.Payments.GetByRazorpayOrderIdAsync(paymentVerificationDto.OrderId);
            if (payment != null)
            {
                payment.Status = isValid ? PaymentStatus.Success : PaymentStatus.Failed;
                payment.RazorpayPaymentId = paymentVerificationDto.PaymentId;
                payment.RazorpaySignature = paymentVerificationDto.Signature;
                payment.UpdatedAt = DateTime.UtcNow;
                if (isValid)
                {
                    payment.PaidAt = DateTime.UtcNow;
                }

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
            }

            await UpdateOrderPaymentStatusAsync(paymentVerificationDto.OrderId, isValid);
            
            return isValid;
        }

        public async Task UpdateOrderPaymentStatusAsync(string orderId, bool isSuccess)
        {
            if (isSuccess)
            {
                await _orderService.UpdateOrderStatusAsync(
                    Guid.Parse(orderId), 
                    OrderStatus.PaymentConfirmed);
            }
            else
            {
                await _orderService.UpdateOrderStatusAsync(
                    Guid.Parse(orderId), 
                    OrderStatus.PaymentFailed);
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