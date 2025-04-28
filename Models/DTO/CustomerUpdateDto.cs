namespace CycleAPI.Models.DTO
{
    public class CustomerUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? MarketingPreferences { get; set; }
        public string? ReferralSource { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
    }
}
