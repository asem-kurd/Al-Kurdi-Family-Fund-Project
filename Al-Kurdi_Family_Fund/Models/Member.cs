namespace Al_Kurdi_Family_Fund.Models
{
    public class Member
    {
        // ─── Primary Key ───────────────────────────────
        // EF Core sees "Id" and automatically makes it
        // the Primary Key (unique number for each row)
        public int Id { get; set; }

        // ─── Personal Information ───────────────────────
        // Required() means this column cannot be empty
        public required string FullName { get; set; }

        public required string Phone { get; set; }

        // Email is optional — not all elderly members have one
        public string? Email { get; set; }

        // ─── Security ───────────────────────────────────
        // We NEVER store the real password
        // BCrypt will turn "1234" into "$2a$11$xyz..." (unreadable)
        public required string PasswordHash { get; set; }

        // ─── Role ────────────────────────────────────────
        // "Admin" or "Member" — controls what they can see
        public string Role { get; set; } = "Member";

        // ─── Status ──────────────────────────────────────
        public bool IsActive { get; set; } = true;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        // ─── Navigation Properties ───────────────────────
        // These are NOT columns in the database
        // They tell EF Core about RELATIONSHIPS between tables
        // "One Member has many Transactions"
        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public ICollection<HostingRequest> HostingRequests { get; set; }
            = new List<HostingRequest>();
    }
}