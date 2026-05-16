namespace Al_Kurdi_Family_Fund.DTOs
{
    // ─────────────────────────────────────────────────────────────
    //  INPUT DTOs  (what the API receives)
    // ─────────────────────────────────────────────────────────────

    // Used by POST /api/transactions — Admin records a payment
    public class CreateTransactionDto
    {
        // "Deposit" or "Withdrawal"
        public required string Type { get; set; }

        // Amount in JD — must be positive
        public decimal Amount { get; set; }

        // e.g. "اشتراك مايو 2026"
        public required string Description { get; set; }

        // Which member is this transaction for?
        public int MemberId { get; set; }

        // Optional: override the date (for back-dating a payment)
        // If null → we default to DateTime.UtcNow in the service
        public DateTime? Date { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  OUTPUT DTOs  (what the API returns)
    // ─────────────────────────────────────────────────────────────

    // Returned after creating OR when listing transactions
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        // "الرصيد بعد" — the running balance your UI already shows
        public decimal BalanceAfter { get; set; }

        // "TXN-0001" style — shown in your UI's reference column
        public string ReferenceNumber { get; set; } = string.Empty;

        // Who this is for — we include name so the frontend
        // doesn't need a second API call just to show the name
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
    }

    // Returned by GET /api/transactions/balance
    public class FundBalanceDto
    {
        // Sum of all Deposits minus sum of all Withdrawals
        public decimal CurrentBalance { get; set; }

        // Useful for the dashboard summary cards
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }

        // How many members paid this month (for the dashboard)
        public int MembersPaidThisMonth { get; set; }

        // Total active members (denominator for the above)
        public int TotalActiveMembers { get; set; }
    }
}