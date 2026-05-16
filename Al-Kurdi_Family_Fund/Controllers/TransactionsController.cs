using Al_Kurdi_Family_Fund.DTOs;
using Al_Kurdi_Family_Fund.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Al_Kurdi_Family_Fund.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]   // all endpoints require a valid JWT — no anonymous access
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // ─── HELPER: extract the logged-in user's ID from the JWT ────────
        // The JWT payload has a claim "sub" = member ID (set in AuthService)
        // We need it to know WHICH admin recorded the transaction
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // ─── HELPER: is the current user an Admin? ───────────────────────
        private bool IsAdmin()
        {
            return User.IsInRole("Admin") ||
                   User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
        }

        // ════════════════════════════════════════════════════════════════
        //  POST /api/transactions
        //  Record a new Deposit or Withdrawal — Admin only
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(
            [FromBody] CreateTransactionDto dto)
        {
            if (!IsAdmin())
                return Forbid();   // 403 — logged in but not Admin

            var adminId = GetCurrentUserId();
            if (adminId == 0)
                return Unauthorized();

            var (success, message, transaction) =
                await _transactionService.CreateTransactionAsync(dto, adminId);

            if (!success)
                return BadRequest(new { message });

            // 201 Created + Location header pointing to the new resource
            return CreatedAtAction(
                nameof(GetTransactions),
                new { memberId = transaction!.MemberId },
                new { message, transaction });
        }

        // ════════════════════════════════════════════════════════════════
        //  GET /api/transactions
        //  All transactions — paginated. Admin only.
        //  Query params: page, pageSize, memberId, type
        // ════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? memberId = null,
            [FromQuery] string? type = null)
        {
            if (!IsAdmin())
                return Forbid();

            var result = await _transactionService
                .GetTransactionsAsync(page, pageSize, memberId, type);

            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════
        //  GET /api/transactions/member/{id}
        //  One member's transaction history — paginated.
        //  Accessible by: Admin (any member) OR the member themselves.
        // ════════════════════════════════════════════════════════════════
        [HttpGet("member/{id:int}")]
        public async Task<IActionResult> GetMemberTransactions(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentUserId();

            // A member can only see their own transactions
            // An admin can see anyone's
            if (!IsAdmin() && currentUserId != id)
                return Forbid();

            var result = await _transactionService
                .GetMemberTransactionsAsync(id, page, pageSize);

            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════
        //  GET /api/transactions/balance
        //  Fund summary — Admin only (contains financial totals)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("balance")]
        public async Task<IActionResult> GetFundBalance()
        {
            if (!IsAdmin())
                return Forbid();

            var balance = await _transactionService.GetFundBalanceAsync();
            return Ok(balance);
        }
    }
}