using B4Budget.Models;

namespace B4Budget.Services;

public class CalculationService : ICalculationService
{
    public decimal GetItemMonthValue(BudgetItem item, int monthOffset)
    {
        // Check if a MonthValue entry exists for this offset
        var monthValue = item.MonthValues
            .FirstOrDefault(mv => mv.MonthOffset == monthOffset);

        if (monthValue is not null)
            return monthValue.Value;

        // No stored value: for Monthly recurrence, fall back to the base amount.
        // For all other recurrence types, no entry = no value for that month.
        return item.Recurrence == RecurrenceType.Monthly
            ? item.Amount
            : 0m;
    }

    public decimal GetMonthTotal(Budget budget, SectionType section, int monthOffset)
    {
        return budget.Items
            .Where(i => i.Section == section)
            .Sum(i => GetItemMonthValue(i, monthOffset));
    }

    public decimal GetMonthNet(Budget budget, int monthOffset)
    {
        var income = GetMonthTotal(budget, SectionType.Income, monthOffset);
        var expense = GetMonthTotal(budget, SectionType.Expense, monthOffset);
        return income - expense;
    }

    public decimal GetRunningBalance(Budget budget, int monthOffset)
    {
        decimal balance = 0;
        for (var i = 0; i <= monthOffset; i++)
        {
            balance += GetMonthNet(budget, i);
        }
        return balance;
    }

    public decimal GetYearEndBalance(Budget budget) => GetRunningBalance(budget, 11);

    public decimal GetItemYearlyTotal(BudgetItem item)
    {
        decimal total = 0;
        for (var i = 0; i < 12; i++)
        {
            total += GetItemMonthValue(item, i);
        }
        return total;
    }

    public decimal GetSectionYearlyTotal(Budget budget, SectionType section)
    {
        return budget.Items
            .Where(i => i.Section == section)
            .Sum(GetItemYearlyTotal);
    }
}