using B4Budget.Models;

namespace B4Budget.Services;

public interface IBudgetService
{
    Task<Budget?> GetActiveBudgetAsync();
    Task<List<Budget>> GetArchivedBudgetsAsync();
    Task<Budget> CreateBudgetAsync(string name, int startMonth, int startYear);
    Task ArchiveBudgetAsync(int budgetId);
    Task<Budget> CreateBudgetFromPreviousAsync(int previousBudgetId, string name, int startMonth, int startYear);
    Task<BudgetItem> AddItemAsync(int budgetId, string name, SectionType section, RecurrenceType recurrence, decimal amount);
    Task UpdateItemAsync(BudgetItem item);
    Task DeleteItemAsync(int itemId);
    Task SetMonthValueAsync(int budgetItemId, int monthOffset, decimal value);
    Task UpdateSortOrderAsync(int budgetItemId, int newSortOrder);
}