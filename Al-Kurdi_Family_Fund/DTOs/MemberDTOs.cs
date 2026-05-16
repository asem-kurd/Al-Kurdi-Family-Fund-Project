using System.ComponentModel.DataAnnotations;

namespace Al_Kurdi_Family_Fund.DTOs
{
    // What we return to the client (never expose PasswordHash)
    public class MemberDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinDate { get; set; }

        // Payment summary — useful for the dashboard
        public decimal TotalPaid { get; set; }
        public bool PaidCurrentMonth { get; set; }
    }

    // What we receive when creating a new member
    public class CreateMemberDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";
    }

    // What we receive when editing a member
    public class UpdateMemberDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
    }

    // Paginated response wrapper — reusable for any list
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;
    }
}