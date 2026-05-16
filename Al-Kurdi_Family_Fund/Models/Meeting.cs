namespace Al_Kurdi_Family_Fund.Models
{
    // One row per monthly meeting
    public class Meeting
    {
        public int Id { get; set; }

        // e.g. "اجتماع مايو 2026"
        public required string Title { get; set; }

        public DateTime Date { get; set; }

        // e.g. "8:00 مساءً"
        public required string Time { get; set; }

        // e.g. "منزل الأخ خالد"
        public string? Location { get; set; }

        // Discussion points
        public string? Agenda { get; set; }

        // "upcoming" | "completed" | "cancelled"
        public string Status { get; set; } = "upcoming";

        // ─── Who is hosting this month? ──────────────────
        // FK → links to the Members table
        public int HostMemberId { get; set; }

        // Navigation property — lets us write:
        // meeting.Host.FullName  (no SQL needed)
        public Member Host { get; set; } = null!;

        // How many members attended (recorded after meeting)
        public int? AttendanceCount { get; set; }
    }
}