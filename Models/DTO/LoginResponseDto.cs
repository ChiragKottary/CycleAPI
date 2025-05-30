﻿namespace CycleAPI.Models.DTO
{
    public class LoginResponseDto
    {
        public string Email { get; set; }

        public List<string> Roles { get; set; }

        public string Token { get; set; }
        public Guid UserId { get; set; }
    }
}
