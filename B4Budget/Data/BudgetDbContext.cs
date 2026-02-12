using B4Budget.Models;
using Microsoft.EntityFrameworkCore;

namespace B4Budget.Data;

public class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<MonthValue> MonthValues => Set<MonthValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Budget
        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);
            e.HasMany(b => b.Items)
                .WithOne(i => i.Budget)
                .HasForeignKey(i => i.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BudgetItem
        modelBuilder.Entity<BudgetItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(200);
            e.Property(i => i.Amount)
                .HasColumnType("TEXT"); // SQLite stores decimals as TEXT
            e.HasMany(i => i.MonthValues)
                .WithOne(mv => mv.BudgetItem)
                .HasForeignKey(mv => mv.BudgetItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MonthValue
        modelBuilder.Entity<MonthValue>(e =>
        {
            e.HasKey(mv => mv.Id);
            e.Property(mv => mv.Value)
                .HasColumnType("TEXT"); // SQLite stores decimals as TEXT
            // Composite unique index: one value per item per month
            e.HasIndex(mv => new { mv.BudgetItemId, mv.MonthOffset })
                .IsUnique();
        });
    }
}