namespace CycleAPI.Models.Domain
{
    public class CartActivityLog
    {
        public Guid LogId { get; set; }
        
        // Foreign Keys
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? CycleId { get; set; }
        public Guid? UserId { get; set; }

        // Properties
        public string Action { get; set; }
        public int? Quantity { get; set; }
        public int? PreviousQuantity { get; set; }
        public string IpAddress { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public Cart Cart { get; set; }
        public Customer Customer { get; set; }
        public Cycle Cycle { get; set; }
        public User User { get; set; }
    }
}
