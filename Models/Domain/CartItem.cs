namespace CycleAPI.Models.Domain
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
        public Guid CycleId { get; set; }
        public Cycle Cycle { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // Price at time of adding to cart
        public decimal Subtotal { get; set; }   // Calculated price * quantity
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
