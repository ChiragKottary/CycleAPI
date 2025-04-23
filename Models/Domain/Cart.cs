namespace CycleAPI.Models.Domain
{
    public class Cart
    {
        public Guid CartId { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsActive { get; set; }
        public string? SessionId { get; set; } // For guest cart tracking
        public string? Notes { get; set; }

        public Guid? LastAccessedByUserId { get; set; }
        public User LastAccessedByUser { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        // Navigation
        public ICollection<CartItem> CartItems { get; set; }
    }
}
