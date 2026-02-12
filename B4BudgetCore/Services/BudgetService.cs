using B4BudgetCore.Data;
using B4BudgetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace B4BudgetCore.Services;

public class BudgetService(BudgetDbContext db) : IBudgetService
{
    public async Task<Budget?> GetActiveBudgetAsync()
    {
        return await db.Budgets
            .Include(budget => budget.Entries)
                .ThenInclude(entry => entry.MonthValues)
            .Where(budget => !budget.IsArchived)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Budget>> GetArchivedBudgetsAsync()
    {
        return await db.Budgets
            .Where(budget => budget.IsArchived)
            .OrderByDescending(budget => budget.CreatedAt)
            .ToListAsync();
    }

    public async Task<Budget> CreateBudgetAsync(string name, int startMonth, int startYear)
    {
        var budget = new Budget
        {
            Name = name,
            StartMonth = startMonth,
            StartYear = startYear
        };

        db.Budgets.Add(budget);
        await db.SaveChangesAsync();
        return budget;
    }

    public async Task ArchiveBudgetAsync(int budgetId)
    {
        var budget = await db.Budgets.FindAsync(budgetId);
        if (budget is null) return;

        budget.IsArchived = true;
        budget.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteBudgetAsync(int budgetId)
    {
        var budget = await db.Budgets.FindAsync(budgetId);
        if (budget is null) return;

        db.Budgets.Remove(budget);
        await db.SaveChangesAsync();
    }

    public async Task<Budget> CreateBudgetFromPreviousAsync(
        int previousBudgetId, string name, int startMonth, int startYear)
    {
        var previous = await db.Budgets
            .Include(budget => budget.Entries)
            .FirstOrDefaultAsync(budget => budget.Id == previousBudgetId);

        // Archive the previous budget
        if (previous is not null)
        {
            previous.IsArchived = true;
            previous.UpdatedAt = DateTime.UtcNow;
        }

        // Create the new budget
        var newBudget = new Budget
        {
            Name = name,
            StartMonth = startMonth,
            StartYear = startYear
        };
        db.Budgets.Add(newBudget);
        await db.SaveChangesAsync(); // Get the new budget's ID

        // Carry over Monthly and Periodic entries (NOT OneOff or Manual)
        if (previous is not null)
        {
            var carryOverEntries = previous.Entries
                .Where(entry => entry.Recurrence is RecurrenceType.Monthly or RecurrenceType.Periodic)
                .ToList();

            foreach (var newEntry in carryOverEntries.Select(oldEntry => new BudgetEntry
                     {
                         BudgetId = newBudget.Id,
                         Name = oldEntry.Name,
                         Section = oldEntry.Section,
                         Recurrence = oldEntry.Recurrence,
                         Amount = oldEntry.Amount,
                         SortOrder = oldEntry.SortOrder
                     }))
            {
                db.BudgetEntries.Add(newEntry);

                // Auto-generate month values for Monthly entries
                if (newEntry.Recurrence != RecurrenceType.Monthly)
                    continue;

                await db.SaveChangesAsync(); // Get the entry ID
                for (var offset = 0; offset < 12; offset++)
                {
                    db.MonthValues.Add(new MonthValue
                    {
                        BudgetEntryId = newEntry.Id,
                        MonthOffset = offset,
                        Value = newEntry.Amount,
                        IsOverride = false
                    });
                }
            }
        }

        await db.SaveChangesAsync();

        // Return fully loaded budget
        return (await GetActiveBudgetAsync())!;
    }

    public async Task<BudgetEntry> AddEntryAsync(
        int budgetId, string name, SectionType section,
        RecurrenceType recurrence, decimal amount)
    {
        // Determine the next sort order in this section
        var maxSort = await db.BudgetEntries
            .Where(entry => entry.BudgetId == budgetId && entry.Section == section)
            .MaxAsync(entry => entry.SortOrder as int?) ?? -1;

        var entry = new BudgetEntry
        {
            BudgetId = budgetId,
            Name = name,
            Section = section,
            Recurrence = recurrence,
            Amount = amount,
            SortOrder = maxSort + 1
        };

        db.BudgetEntries.Add(entry);
        await db.SaveChangesAsync(); // Get the entry ID

        if (recurrence != RecurrenceType.Monthly)
            return entry;

        // Auto-generate month values for Monthly recurrence
        for (var offset = 0; offset < 12; offset++)
        {
            db.MonthValues.Add(new MonthValue
            {
                BudgetEntryId = entry.Id,
                MonthOffset = offset,
                Value = amount,
                IsOverride = false
            });
        }
        await db.SaveChangesAsync();

        return entry;
    }

    public async Task UpdateEntryAsync(BudgetEntry entry)
    {
        var existing = await db.BudgetEntries.FindAsync(entry.Id);
        if (existing is null) return;

        existing.Name = entry.Name;
        existing.Amount = entry.Amount;
        existing.Recurrence = entry.Recurrence;
        existing.SortOrder = entry.SortOrder;
        await db.SaveChangesAsync();
    }

    public async Task DeleteEntryAsync(int entryId)
    {
        var entry = await db.BudgetEntries
            .Include(entry => entry.MonthValues)
            .FirstOrDefaultAsync(entry => entry.Id == entryId);

        if (entry is null) return;

        db.BudgetEntries.Remove(entry); // Cascade deletes MonthValues
        await db.SaveChangesAsync();
    }

    public async Task SetMonthValueAsync(int budgetEntryId, int monthOffset, decimal value)
    {
        var existing = await db.MonthValues
            .FirstOrDefaultAsync(monthValue =>
                monthValue.BudgetEntryId == budgetEntryId && monthValue.MonthOffset == monthOffset);

        if (existing is not null)
        {
            existing.Value = value;
            existing.IsOverride = true;
        }
        else
        {
            db.MonthValues.Add(new MonthValue
            {
                BudgetEntryId = budgetEntryId,
                MonthOffset = monthOffset,
                Value = value,
                IsOverride = true // User is explicitly placing a value
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(int budgetEntryId, int newSortOrder)
    {
        var entry = await db.BudgetEntries.FindAsync(budgetEntryId);
        if (entry is null) return;

        entry.SortOrder = newSortOrder;
        await db.SaveChangesAsync();
    }
}