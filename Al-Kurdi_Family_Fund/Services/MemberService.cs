using Al_Kurdi_Family_Fund.Data;
using Al_Kurdi_Family_Fund.DTOs;
using Al_Kurdi_Family_Fund.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Kurdi_Family_Fund.Services
{
    public class MemberService
    {
        private readonly AppDbContext _db;

        public MemberService(AppDbContext db)
        {
            _db = db;
        }

        // ─── GET ALL (paginated + search) ───────────────────────────────
        public async Task<PagedResult<MemberDto>> GetMembersAsync(
            int page, int pageSize, string? search)
        {
            // Start with all members — no data loaded yet, just a query
            var query = _db.Members.AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(m =>
                    m.FullName.ToLower().Contains(search) ||
                    m.Phone.Contains(search) ||
                    (m.Email != null && m.Email.ToLower().Contains(search)));
            }

            // Get total count BEFORE paging (for TotalPages calculation)
            var totalCount = await query.CountAsync();

            // Apply pagination — skip previous pages, take current page only
            var members = await query
                .OrderBy(m => m.FullName)       // alphabetical order
                .Skip((page - 1) * pageSize)    // e.g. page 2 skips first 20
                .Take(pageSize)                 // e.g. take next 20
                .Select(m => new MemberDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Phone = m.Phone,
                    Email = m.Email,
                    Role = m.Role,
                    IsActive = m.IsActive,
                    JoinDate = m.JoinDate,
                    // Sum all approved transactions for this member
                    // REPLACE with this:
                    TotalPaid = m.Transactions
                        .Where(t => t.Type == "Deposit")
                        .Sum(t => t.Amount),
                    PaidCurrentMonth = m.Transactions.Any(t =>
                        t.Type == "Deposit" &&
                        t.Date.Month == DateTime.UtcNow.Month &&
                        t.Date.Year == DateTime.UtcNow.Year)
                })
                .ToListAsync();

            return new PagedResult<MemberDto>
            {
                Items = members,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── GET SINGLE MEMBER ──────────────────────────────────────────
        public async Task<MemberDto?> GetMemberByIdAsync(int id)
        {
            return await _db.Members
                .Where(m => m.Id == id)
                .Select(m => new MemberDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Phone = m.Phone,
                    Email = m.Email,
                    Role = m.Role,
                    IsActive = m.IsActive,
                    JoinDate = m.JoinDate,
                    // REPLACE with this:
                    TotalPaid = m.Transactions
                        .Where(t => t.Type == "Deposit")
                        .Sum(t => t.Amount),
                    PaidCurrentMonth = m.Transactions.Any(t =>
                        t.Type == "Deposit" &&
                        t.Date.Month == DateTime.UtcNow.Month &&
                        t.Date.Year == DateTime.UtcNow.Year)
                })
                .FirstOrDefaultAsync();
        }

        // ─── CREATE MEMBER ──────────────────────────────────────────────
        public async Task<(bool Success, string Message, MemberDto? Member)>
            CreateMemberAsync(CreateMemberDto dto)
        {
            // Check phone is not already taken
            bool phoneExists = await _db.Members
                .AnyAsync(m => m.Phone == dto.Phone);
            if (phoneExists)
                return (false, "رقم الهاتف مستخدم مسبقاً", null);

            // Check email is not already taken (if provided)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool emailExists = await _db.Members
                    .AnyAsync(m => m.Email == dto.Email);
                if (emailExists)
                    return (false, "البريد الإلكتروني مستخدم مسبقاً", null);
            }

            var member = new Member
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role ?? "Member",
                IsActive = true,
                JoinDate = DateTime.UtcNow
            };

            _db.Members.Add(member);
            await _db.SaveChangesAsync();

            var result = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Phone = member.Phone,
                Email = member.Email,
                Role = member.Role,
                IsActive = member.IsActive,
                JoinDate = member.JoinDate,
                TotalPaid = 0,
                PaidCurrentMonth = false
            };

            return (true, "تم إنشاء العضو بنجاح", result);
        }

        // ─── UPDATE MEMBER ──────────────────────────────────────────────
        public async Task<(bool Success, string Message)>
            UpdateMemberAsync(int id, UpdateMemberDto dto)
        {
            var member = await _db.Members.FindAsync(id);
            if (member == null)
                return (false, "العضو غير موجود");

            // Only update fields that were actually sent
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                member.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                // Make sure new phone isn't taken by someone else
                bool taken = await _db.Members
                    .AnyAsync(m => m.Phone == dto.Phone && m.Id != id);
                if (taken)
                    return (false, "رقم الهاتف مستخدم من عضو آخر");

                member.Phone = dto.Phone;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
                member.Email = dto.Email;

            if (dto.IsActive.HasValue)
                member.IsActive = dto.IsActive.Value;

            await _db.SaveChangesAsync();
            return (true, "تم تحديث بيانات العضو بنجاح");
        }
    }
}