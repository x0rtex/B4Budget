using B4BudgetCore.Models;

namespace B4BudgetCore.Services;

public interface IBudgetService
{
    // Budget
    Task<Budget?> GetActiveBudgetAsync();
    Task<List<Budget>> GetArchivedBudgetsAsync();
    Task<Budget> CreateBudgetAsync(string name, int startMonth, int startYear);
    Task ArchiveBudgetAsync(int budgetId);
    Task DeleteBudgetAsync(int budgetId);
    Task<Budget> CreateBudgetFromPreviousAsync(int previousBudgetId, string name, int startMonth, int startYear);

    // Entry
    Task<BudgetEntry> AddEntryAsync(int budgetId, string name, SectionType section, RecurrenceType recurrence, decimal amount);
    Task UpdateEntryAsync(BudgetEntry entry);
    Task DeleteEntryAsync(int entryId);

    // Month Value
    Task SetMonthValueAsync(int budgetEntryId, int monthOffset, decimal value);

    // Sort Order
    Task UpdateSortOrderAsync(int budgetEntryId, int newSortOrder);
}