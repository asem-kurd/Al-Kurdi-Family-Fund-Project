namespace Al_Kurdi_Family_Fund.Models
{
    // When a member wants to HOST a future meeting,
    // they submit a request. Admin approves or rejects it.
    public class HostingRequest
    {
        public int Id { get; set; }

        // ─── Which member is requesting to host? ─────────
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        // ─── Which month do they want to host? ───────────
        // e.g. "2026-07"  (stored as text, easy to display)
        public required string RequestedMonth { get; set; }

        // ─── Status ──────────────────────────────────────
        // "pending" | "approved" | "rejected"
        public string Status { get; set; } = "pending";

        // Admin can leave a note e.g. "Already taken by Ahmed"
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}