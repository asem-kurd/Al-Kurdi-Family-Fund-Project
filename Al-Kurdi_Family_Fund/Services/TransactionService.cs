using Al_Kurdi_Family_Fund.Data;
using Al_Kurdi_Family_Fund.DTOs;
using Al_Kurdi_Family_Fund.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Kurdi_Family_Fund.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _db;

        public TransactionService(AppDbContext db)
        {
            _db = db;
        }

        // ─── CREATE TRANSACTION ──────────────────────────────────────────
        // Called by Admin only. Records a Deposit or Withdrawal.
        public async Task<(bool Success, string Message, TransactionDto? Transaction)>
            CreateTransactionAsync(CreateTransactionDto dto, int adminId)
        {
            // 1. Verify the member exists and is active
            var member = await _db.Members.FindAsync(dto.MemberId);
            if (member == null)
                return (false, "العضو غير موجود", null);
            if (!member.IsActive)
                return (false, "لا يمكن تسجيل معاملة لعضو غير نشط", null);

            // 2. Validate type
            if (dto.Type != "Deposit" && dto.Type != "Withdrawal")
                return (false, "نوع المعاملة يجب أن يكون Deposit أو Withdrawal", null);

            // 3. Validate amount
            if (dto.Amount <= 0)
                return (false, "المبلغ يجب أن يكون أكبر من صفر", null);

            // 4. Calculate the current fund balance BEFORE this transaction.
            //    We sum ALL transactions in the table — this is the source of truth.
            //    Deposits add to balance, Withdrawals subtract from it.
            var currentBalance = await _db.Transactions
                .SumAsync(t => t.Type == "Deposit" ? t.Amount : -t.Amount);

            // 5. Calculate what the balance will be AFTER this transaction
            var balanceAfter = dto.Type == "Deposit"
                ? currentBalance + dto.Amount
                : currentBalance - dto.Amount;

            // 6. Prevent the fund from going negative
            if (balanceAfter < 0)
                return (false, $"رصيد الصندوق غير كافٍ. الرصيد الحالي: {currentBalance:F2} د.أ", null);

            // 7. Generate the reference number: TXN-0001, TXN-0002, etc.
            //    We count existing rows + 1 to get the next number.
            //    NOTE: This is safe for our scale (~1000 members).
            var nextId = await _db.Transactions.CountAsync() + 1;
            var referenceNumber = $"TXN-{nextId:D4}";

            // 8. Build the entity
            var transaction = new Transaction
            {
                Type = dto.Type,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date?.ToUniversalTime() ?? DateTime.UtcNow,
                BalanceAfter = balanceAfter,
                ReferenceNumber = referenceNumber,
                MemberId = dto.MemberId,
                RecordedByAdminId = adminId
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            // 9. Return the full DTO (including member name for the UI)
            var result = new TransactionDto
            {
                Id = transaction.Id,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Date = transaction.Date,
                BalanceAfter = transaction.BalanceAfter,
                ReferenceNumber = transaction.ReferenceNumber,
                MemberId = transaction.MemberId,
                MemberName = member.FullName
            };

            return (true, "تم تسجيل المعاملة بنجاح", result);
        }

        // ─── GET ALL TRANSACTIONS (paginated) ───────────────────────────
        // Admin-facing. Can filter by memberId or type.
        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(
            int page, int pageSize, int? memberId, string? type)
        {
            var query = _db.Transactions
                .Include(t => t.Member)   // needed for MemberName
                .AsQueryable();

            // Optional filters
            if (memberId.HasValue)
                query = query.Where(t => t.MemberId == memberId.Value);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(t => t.Type == type);

            var totalCount = await query.CountAsync();

            // Most recent first — that's what your UI shows
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    Description = t.Description,
                    Date = t.Date,
                    BalanceAfter = t.BalanceAfter,
                    ReferenceNumber = t.ReferenceNumber,
                    MemberId = t.MemberId,
                    MemberName = t.Member.FullName
                })
                .ToListAsync();

            return new PagedResult<TransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── GET TRANSACTIONS FOR ONE MEMBER ────────────────────────────
        // Used by member's own history page — and by Admin viewing a member.
        public async Task<PagedResult<TransactionDto>> GetMemberTransactionsAsync(
            int memberId, int page, int pageSize)
        {
            var member = await _db.Members.FindAsync(memberId);
            if (member == null)
                return new PagedResult<TransactionDto>
                {
                    Items = [],
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };

            var query = _db.Transactions
                .Where(t => t.MemberId == memberId);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    Description = t.Description,
                    Date = t.Date,
                    BalanceAfter = t.BalanceAfter,
                    ReferenceNumber = t.ReferenceNumber,
                    MemberId = t.MemberId,
                    MemberName = member.FullName
                })
                .ToListAsync();

            return new PagedResult<TransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── GET FUND BALANCE ────────────────────────────────────────────
        public async Task<FundBalanceDto> GetFundBalanceAsync()
        {
            var totals = await _db.Transactions
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalDeposits = g.Where(t => t.Type == "Deposit")
                                        .Sum(t => t.Amount),
                    TotalWithdrawals = g.Where(t => t.Type == "Withdrawal")
                                        .Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync();

            var deposits = totals?.TotalDeposits ?? 0;
            var withdrawals = totals?.TotalWithdrawals ?? 0;

            var now = DateTime.UtcNow;
            var paidThisMonth = await _db.Transactions
                .Where(t =>
                    t.Type == "Deposit" &&
                    t.Date.Month == now.Month &&
                    t.Date.Year == now.Year)
                .Select(t => t.MemberId)
                .Distinct()
                .CountAsync();

            var totalActive = await _db.Members
                .CountAsync(m => m.IsActive);

            return new FundBalanceDto
            {
                CurrentBalance = deposits - withdrawals,
                TotalDeposits = deposits,
                TotalWithdrawals = withdrawals,
                MembersPaidThisMonth = paidThisMonth,
                TotalActiveMembers = totalActive
            };
        }
    }
}