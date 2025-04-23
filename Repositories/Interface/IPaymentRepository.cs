using CycleAPI.Models.Domain;

namespace CycleAPI.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid paymentId);
        Task<Payment?> GetByRazorpayOrderIdAsync(string razorpayOrderId);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId);
    }
}