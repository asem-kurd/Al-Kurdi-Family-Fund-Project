using Al_Kurdi_Family_Fund.DTOs;
using Al_Kurdi_Family_Fund.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Kurdi_Family_Fund.Controllers
{
    [ApiController]
    [Route("api/members")]
    [Authorize] // ALL endpoints require a valid JWT token
    public class MembersController : ControllerBase
    {
        private readonly MemberService _memberService;

        public MembersController(MemberService memberService)
        {
            _memberService = memberService;
        }

        // ─── GET /api/members?page=1&pageSize=20&search=أحمد ────────────
        // Returns paginated list — handles 1000 members efficiently
        [HttpGet]
        public async Task<IActionResult> GetMembers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            // Guard against bad input
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var result = await _memberService.GetMembersAsync(page, pageSize, search);
            return Ok(result);
        }

        // ─── GET /api/members/5 ──────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);

            if (member == null)
                return NotFound(new { message = "العضو غير موجود" });

            return Ok(member);
        }

        // ─── POST /api/members ───────────────────────────────────────────
        // Admin only — regular members cannot create new accounts
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMember([FromBody] CreateMemberDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, member) =
                await _memberService.CreateMemberAsync(dto);

            if (!success)
                return BadRequest(new { message });

            // 201 Created + Location header pointing to the new member
            return CreatedAtAction(
                nameof(GetMember),
                new { id = member!.Id },
                member);
        }

        // ─── PUT /api/members/5 ──────────────────────────────────────────
        // Admin only — update name, phone, email, or active status
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMember(
            int id, [FromBody] UpdateMemberDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) =
                await _memberService.UpdateMemberAsync(id, dto);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}