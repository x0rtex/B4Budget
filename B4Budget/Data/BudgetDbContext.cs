using B4Budget.Models;
using Microsoft.EntityFrameworkCore;

namespace B4Budget.Data;

public class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();
    public DbSet<MonthValue> MonthValues => Set<MonthValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Budget
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(budget => budget.Id);
            entity.Property(budget => budget.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasMany(budget => budget.Entries)
                .WithOne(entry => entry.Budget)
                .HasForeignKey(entry => entry.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BudgetEntry
        modelBuilder.Entity<BudgetEntry>(entity =>
        {
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(entry => entry.Amount)
                .HasColumnType("TEXT"); // SQLite stores decimals as TEXT
            entity.HasMany(entry => entry.MonthValues)
                .WithOne(monthValue => monthValue.BudgetEntry)
                .HasForeignKey(monthValue => monthValue.BudgetEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MonthValue
        modelBuilder.Entity<MonthValue>(entity =>
        {
            entity.HasKey(monthValue => monthValue.Id);
            entity.Property(monthValue => monthValue.Value)
                .HasColumnType("TEXT"); // SQLite stores decimals as TEXT
            // Composite unique index: one value per entry per month
            entity.HasIndex(monthValue => new { monthValue.BudgetEntryId, monthValue.MonthOffset })
                .IsUnique();
        });
    }
}