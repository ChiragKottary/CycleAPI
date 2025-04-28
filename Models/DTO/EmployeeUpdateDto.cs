namespace CycleAPI.Models.DTO
{
    public class EmployeeUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public Guid? RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}