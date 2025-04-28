namespace CycleAPI.Models.DTO
{
    public class CustomerCreateDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? MarketingPreferences { get; set; }
        public string? ReferralSource { get; set; }
        public required string Password { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
