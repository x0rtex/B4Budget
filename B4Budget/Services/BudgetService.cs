using B4Budget.Data;
using B4Budget.Models;
using Microsoft.EntityFrameworkCore;

namespace B4Budget.Services;

public class BudgetService(BudgetDbContext db)
{
    public async Task<Budget?> GetActiveBudgetAsync()
    {
        return await db.Budgets
            .Include(b => b.Items)
                .ThenInclude(i => i.MonthValues)
            .Where(b => !b.IsArchived)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Budget>> GetArchivedBudgetsAsync()
    {
        return await db.Budgets
            .Where(b => b.IsArchived)
            .OrderByDescending(b => b.CreatedAt)
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

    public async Task<Budget> CreateBudgetFromPreviousAsync(
        int previousBudgetId, string name, int startMonth, int startYear)
    {
        var previous = await db.Budgets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == previousBudgetId);

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

        // Carry over Monthly and Periodic items (NOT OneOff or Manual)
        if (previous is not null)
        {
            var carryOverItems = previous.Items
                .Where(i => i.Recurrence is RecurrenceType.Monthly or RecurrenceType.Periodic)
                .ToList();

            foreach (var newItem in carryOverItems.Select(oldItem => new BudgetItem
                     {
                         BudgetId = newBudget.Id,
                         Name = oldItem.Name,
                         Section = oldItem.Section,
                         Recurrence = oldItem.Recurrence,
                         Amount = oldItem.Amount,
                         SortOrder = oldItem.SortOrder
                     }))
            {
                db.BudgetItems.Add(newItem);

                // Auto-generate month values for Monthly items
                if (newItem.Recurrence != RecurrenceType.Monthly)
                    continue;

                await db.SaveChangesAsync(); // Get the item ID
                for (var offset = 0; offset < 12; offset++)
                {
                    db.MonthValues.Add(new MonthValue
                    {
                        BudgetItemId = newItem.Id,
                        MonthOffset = offset,
                        Value = newItem.Amount,
                        IsOverride = false
                    });
                }
            }
        }

        await db.SaveChangesAsync();

        // Return fully loaded budget
        return (await GetActiveBudgetAsync())!;
    }

    public async Task<BudgetItem> AddItemAsync(
        int budgetId, string name, SectionType section,
        RecurrenceType recurrence, decimal amount)
    {
        // Determine the next sort order in this section
        var maxSort = await db.BudgetItems
            .Where(i => i.BudgetId == budgetId && i.Section == section)
            .MaxAsync(i => (int?)i.SortOrder) ?? -1;

        var item = new BudgetItem
        {
            BudgetId = budgetId,
            Name = name,
            Section = section,
            Recurrence = recurrence,
            Amount = amount,
            SortOrder = maxSort + 1
        };

        db.BudgetItems.Add(item);
        await db.SaveChangesAsync(); // Get the item ID

        if (recurrence != RecurrenceType.Monthly)
            return item;

        // Auto-generate month values for Monthly recurrence
        for (var offset = 0; offset < 12; offset++)
        {
            db.MonthValues.Add(new MonthValue
            {
                BudgetItemId = item.Id,
                MonthOffset = offset,
                Value = amount,
                IsOverride = false
            });
        }
        await db.SaveChangesAsync();

        return item;
    }

    public async Task UpdateItemAsync(BudgetItem item)
    {
        var existing = await db.BudgetItems.FindAsync(item.Id);
        if (existing is null) return;

        existing.Name = item.Name;
        existing.Amount = item.Amount;
        existing.Recurrence = item.Recurrence;
        existing.SortOrder = item.SortOrder;
        await db.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int itemId)
    {
        var item = await db.BudgetItems
            .Include(i => i.MonthValues)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item is null) return;

        db.BudgetItems.Remove(item); // Cascade deletes MonthValues
        await db.SaveChangesAsync();
    }

    public async Task SetMonthValueAsync(int budgetItemId, int monthOffset, decimal value)
    {
        var existing = await db.MonthValues
            .FirstOrDefaultAsync(mv =>
                mv.BudgetItemId == budgetItemId && mv.MonthOffset == monthOffset);

        if (existing is not null)
        {
            existing.Value = value;
            existing.IsOverride = true;
        }
        else
        {
            db.MonthValues.Add(new MonthValue
            {
                BudgetItemId = budgetItemId,
                MonthOffset = monthOffset,
                Value = value,
                IsOverride = true // User is explicitly placing a value
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(int budgetItemId, int newSortOrder)
    {
        var item = await db.BudgetItems.FindAsync(budgetItemId);
        if (item is null) return;

        item.SortOrder = newSortOrder;
        await db.SaveChangesAsync();
    }
}