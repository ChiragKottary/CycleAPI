namespace CycleAPI.Models.DTO
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasActiveCart { get; set; }
        public int TotalOrders { get; set; }
    }
}
