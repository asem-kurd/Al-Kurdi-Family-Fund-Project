using System.ComponentModel.DataAnnotations;

namespace Al_Kurdi_Family_Fund.DTOs
{
    // ─── What the frontend SENDS to login ────────────────
    // This matches exactly what your shared.js sends:
    // return this.post('/api/auth/login', { email, password });
    public class LoginRequestDto
    {
        [Required]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // ─── What the API RETURNS after successful login ──────
    // This is what Auth.save(token, user) in your JS receives
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public required UserInfoDto User { get; set; }
    }

    // ─── The user info stored in localStorage ─────────────
    // Matches exactly what your shared.js expects:
    // { id, fullName, email, role, familyId }
    public class UserInfoDto
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Email { get; set; }
        public required string Phone { get; set; }
        public required string Role { get; set; }
    }
}