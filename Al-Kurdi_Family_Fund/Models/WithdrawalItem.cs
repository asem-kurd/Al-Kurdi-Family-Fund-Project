namespace Al_Kurdi_Family_Fund.Models
{
    // This is one LINE in the withdrawal breakdown.
    // Example: Description="كؤوس ماء"  Amount=22.00
    // Example: Description="تمر"        Amount=50.00
    public class WithdrawalItem
    {
        public int Id { get; set; }

        // ─── What was purchased / spent on ───────────────
        public required string Description { get; set; }

        public decimal Amount { get; set; }

        // ─── Which withdrawal does this line belong to? ──
        // FK → links back to the Withdrawal header
        public int WithdrawalId { get; set; }

        // Navigation property
        public Withdrawal Withdrawal { get; set; } = null!;
    }
}