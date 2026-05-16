namespace Al_Kurdi_Family_Fund.Models
{
    // A withdrawal is an emergency payment FROM the fund.
    // It has a HEADER (this class) and LINE ITEMS (WithdrawalItem.cs)
    // Example header: "مساعدة وفاة - والد أبو سالم - 483 د.أ"
    // Example items:  "كؤوس ماء - 22 د.أ" , "تمر - 50 د.أ"
    public class Withdrawal
    {
        public int Id { get; set; }

        // ─── Reference number ────────────────────────────
        // e.g. "WTH-0001"
        public required string ReferenceNumber { get; set; }

        // ─── Basic info shown in withdrawals.html ────────
        public required string Title { get; set; }

        public required string BeneficiaryName { get; set; }

        public required string Reason { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // ─── Type of emergency ───────────────────────────
        // "death" | "emergency" | "medical" | "other"
        public required string Type { get; set; }

        // ─── Status ──────────────────────────────────────
        // "pending" | "approved" | "rejected"
        public string Status { get; set; } = "pending";

        // ─── Total is CALCULATED from line items ─────────
        // We store it here for quick display
        public decimal TotalAmount { get; set; }

        // ─── Who approved this withdrawal? ───────────────
        public int? ApprovedByAdminId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // ─── Line items (the details breakdown) ──────────
        // "One Withdrawal has many WithdrawalItems"
        public ICollection<WithdrawalItem> Items { get; set; }
            = new List<WithdrawalItem>();
    }
}