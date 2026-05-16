namespace Al_Kurdi_Family_Fund.Models
{
    // The 🔔 bell icon in your navbar.
    // Every alert a member sees is one row here.
    public class Notification
    {
        public int Id { get; set; }

        // ─── Who receives this notification? ─────────────
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        // ─── The message shown to the user ───────────────
        // e.g. "تم تسجيل دفعتك لشهر مايو"
        public required string Message { get; set; }

        // ─── Type controls the icon shown ────────────────
        // "payment" | "withdrawal" | "meeting" | "general"
        public string Type { get; set; } = "general";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}