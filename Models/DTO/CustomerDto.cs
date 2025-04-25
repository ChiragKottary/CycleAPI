using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class CustomerDto
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasActiveCart { get; set; }
        public int TotalOrders { get; set; }
        public CartDto? ActiveCart { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
