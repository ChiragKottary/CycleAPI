namespace CycleAPI.Models.DTO
{
    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}