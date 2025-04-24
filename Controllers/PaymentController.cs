using CycleAPI.Models.DTO;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IRazorpayService _razorpayService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IRazorpayService razorpayService,
            ILogger<PaymentController> logger)
        {
            _razorpayService = razorpayService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentOrderResponseDto>> CreatePaymentOrder([FromBody] PaymentCreateRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Creating payment order for Order ID: {request.OrderId}");
                var paymentOrder = await _razorpayService.CreatePaymentOrderAsync(request.OrderId);
                return Ok(paymentOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error creating payment order: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> VerifyPayment([FromBody] PaymentVerificationDto paymentVerification)
        {
            try
            {
                _logger.LogInformation($"Verifying payment for Order ID: {paymentVerification.OrderId}");
                var isValid = await _razorpayService.VerifyPaymentAsync(paymentVerification);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying payment: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}