namespace Al_Kurdi_Family_Fund.Models
{
    // Every time money moves IN or OUT of the fund,
    // one row is added to this table. It is PERMANENT — 
    // no deleting, no editing. Exactly like your current UI says.
    public class Transaction
    {
        public int Id { get; set; }

        // ─── What kind of movement? ──────────────────────
        // "Deposit"    → member pays monthly subscription
        // "Withdrawal" → money leaves the fund for an emergency
        public required string Type { get; set; }

        // ─── The amount in Jordanian Dinars ─────────────
        public decimal Amount { get; set; }

        // ─── Description shown in the UI ────────────────
        // e.g. "اشتراك مايو 2026"
        public required string Description { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // ─── Running balance AFTER this transaction ──────
        // Your UI already shows this column "الرصيد بعد"
        public decimal BalanceAfter { get; set; }

        // ─── Reference number shown in UI ───────────────
        // e.g. "TXN-0001"
        public required string ReferenceNumber { get; set; }

        // ─── Who is this transaction FOR? ───────────────
        // FK = Foreign Key → links to the Members table
        public int MemberId { get; set; }

        // Navigation property — lets us write:
        // transaction.Member.FullName  (no SQL JOIN needed)
        public Member Member { get; set; } = null!;

        // ─── Who recorded this in the system? ───────────
        public int RecordedByAdminId { get; set; }
    }
}