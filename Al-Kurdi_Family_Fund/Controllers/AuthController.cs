using Al_Kurdi_Family_Fund.DTOs;
using Al_Kurdi_Family_Fund.Services;
using Microsoft.AspNetCore.Mvc;

namespace Al_Kurdi_Family_Fund.Controllers
{
    // [ApiController] → enables automatic validation
    // [Route]         → sets the URL prefix for all methods here
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ─── POST /api/auth/login ─────────────────────────
        // This is what your login.html calls
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                // Return 401 with Arabic message matching your UI
                return Unauthorized(new { message = "البريد أو كلمة المرور غير صحيحة" });
            }

            return Ok(result);
        }

        // ─── POST /api/auth/setup-admin ───────────────────
        // ONE TIME USE — creates the first admin account
        // We will disable this after first use
            [HttpPost("setup-admin")]
            public async Task<IActionResult> SetupAdmin([FromBody] SetupAdminDto request)
            {
                var result = await _authService.CreateAdminAsync(
                    request.FullName,
                    request.Phone,
                    request.Password,
                    request.Email
                );

                if (!result)
                    return BadRequest(new { message = "الهاتف مستخدم مسبقاً" });

                return Ok(new { message = "تم إنشاء حساب المدير بنجاح" });
            }
    }

    // DTO used only by setup-admin endpoint
    public class SetupAdminDto
    {
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
    }
}