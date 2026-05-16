using Al_Kurdi_Family_Fund.Data;
using Al_Kurdi_Family_Fund.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
//add by me
using BCrypt.Net;

namespace Al_Kurdi_Family_Fund.Services
{
    public class AuthService
    {
        // ─── Dependencies injected by ASP.NET ─────────────
        // Think of these as tools this service needs to work
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ─── Main Login Method ────────────────────────────
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Step 1: Find the member by email OR phone
            // This supports elderly users who remember phone but not email
            var member = await _db.Members
                .Where(m => m.IsActive &&
                           (m.Email == request.EmailOrPhone || m.Phone == request.EmailOrPhone))
                .FirstOrDefaultAsync();

            // Step 2: If not found → return null (wrong credentials)
            if (member == null) return null;

            // Step 3: Check the password
            // BCrypt compares "Admin1234" with the stored hash
            // It never decrypts — it re-hashes and compares
            bool passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                member.PasswordHash
            );

            if (!passwordValid) return null;

            // Step 4: Create the JWT token
            var token = GenerateJwtToken(member.Id, member.FullName, member.Role);

            // Step 5: Return the token + user info
            return new LoginResponseDto
            {
                Token = token,
                User = new UserInfoDto
                {
                    Id = member.Id,
                    FullName = member.FullName,
                    Email = member.Email,
                    Phone = member.Phone,
                    Role = member.Role
                }
            };
        }

        // ─── JWT Token Generator ──────────────────────────
        private string GenerateJwtToken(int memberId, string fullName, string role)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
            );

            // Claims = pieces of info baked INTO the token
            // Your frontend can read these without calling the server
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, memberId.ToString()),
                new Claim(ClaimTypes.Name,           fullName),
                new Claim(ClaimTypes.Role,           role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                                        int.Parse(jwtSettings["ExpiryDays"]!)),
                signingCredentials: new SigningCredentials(
                                        key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─── Helper: Create First Admin Account ───────────
        // We'll call this once to seed the database with an admin
        public async Task<bool> CreateAdminAsync(
            string fullName, string phone, string password, string? email = null)
        {
            // Check if phone already exists
            bool exists = await _db.Members.AnyAsync(m => m.Phone == phone);
            if (exists) return false;

            var admin = new Models.Member
            {
                FullName = fullName,
                Phone = phone,
                Email = email,
                // Hash the password before storing — NEVER store plain text
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Admin",
                IsActive = true,
                JoinDate = DateTime.UtcNow
            };

            _db.Members.Add(admin);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}