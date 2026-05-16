using Al_Kurdi_Family_Fund.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Kurdi_Family_Fund.Data
{
    public class AppDbContext : DbContext
    {
        // This constructor is required — it passes settings
        // (like the connection string) into the base DbContext class
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ─── DbSets ──────────────────────────────────────
        // Each DbSet = one table in your database
        // DbSet<Member> tells EF Core: "create a Members table"
        public DbSet<Member> Members { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<WithdrawalItem> WithdrawalItems { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<HostingRequest> HostingRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // ─── OnModelCreating ─────────────────────────────
        // This is where we give EF Core extra instructions
        // about how to build the tables
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Members table rules ──────────────────────
            modelBuilder.Entity<Member>(entity =>
            {
                // Phone must be unique — no two members share a number
                entity.HasIndex(m => m.Phone).IsUnique();

                // Email must be unique IF provided
                entity.HasIndex(m => m.Email).IsUnique();

                // Amount columns: always 2 decimal places
                // e.g. 3.00 not 3.0000000001
            });

            // ── Transaction table rules ──────────────────
            modelBuilder.Entity<Transaction>(entity =>
            {
                // Store amounts with precision: 18 digits, 2 decimals
                entity.Property(t => t.Amount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(t => t.BalanceAfter)
                      .HasColumnType("decimal(18,2)");

                // Transactions CANNOT be deleted when a member is deleted
                // (financial records must be permanent)
                entity.HasOne(t => t.Member)
                      .WithMany(m => m.Transactions)
                      .HasForeignKey(t => t.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Withdrawal table rules ───────────────────
            modelBuilder.Entity<Withdrawal>(entity =>
            {
                entity.Property(w => w.TotalAmount)
                      .HasColumnType("decimal(18,2)");
            });

            // ── WithdrawalItem table rules ───────────────
            modelBuilder.Entity<WithdrawalItem>(entity =>
            {
                entity.Property(i => i.Amount)
                      .HasColumnType("decimal(18,2)");

                // If a Withdrawal is deleted, delete its items too
                entity.HasOne(i => i.Withdrawal)
                      .WithMany(w => w.Items)
                      .HasForeignKey(i => i.WithdrawalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Meeting table rules ──────────────────────
            modelBuilder.Entity<Meeting>(entity =>
            {
                // A meeting's host member cannot be deleted
                // while they have meetings assigned
                entity.HasOne(m => m.Host)
                      .WithMany()
                      .HasForeignKey(m => m.HostMemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── HostingRequest table rules ───────────────
            modelBuilder.Entity<HostingRequest>(entity =>
            {
                entity.HasOne(h => h.Member)
                      .WithMany(m => m.HostingRequests)
                      .HasForeignKey(h => h.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Notification table rules ─────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.Member)
                      .WithMany(m => m.Notifications)
                      .HasForeignKey(n => n.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}