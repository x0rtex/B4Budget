using B4BudgetCore.Models;

namespace B4BudgetCore.Services;

public class CalculationService : ICalculationService
{
    public decimal GetEntryMonthValue(BudgetEntry entry, int monthOffset)
    {
        // Check if a MonthValue entry exists for this offset
        var monthValue = entry.MonthValues
            .FirstOrDefault(monthValue => monthValue.MonthOffset == monthOffset);

        if (monthValue is not null)
            return monthValue.Value;

        // No stored value: for Monthly recurrence, fall back to the base amount.
        // For all other recurrence types, no entry = no value for that month.
        return entry.Recurrence == RecurrenceType.Monthly
            ? entry.Amount
            : 0m;
    }

    public decimal GetMonthTotal(Budget budget, SectionType section, int monthOffset)
    {
        return budget.Entries
            .Where(entry => entry.Section == section)
            .Sum(entry => GetEntryMonthValue(entry, monthOffset));
    }

    public decimal GetMonthSurplus(Budget budget, int monthOffset)
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
            balance += GetMonthSurplus(budget, i);
        }
        return balance;
    }

    public decimal GetYearEndBalance(Budget budget) => GetRunningBalance(budget, 11);

    public decimal GetEntryYearlyTotal(BudgetEntry entry)
    {
        decimal total = 0;
        for (var month = 0; month < 12; month++)
        {
            total += GetEntryMonthValue(entry, month);
        }
        return total;
    }

    public decimal GetSectionYearlyTotal(Budget budget, SectionType section)
    {
        return budget.Entries
            .Where(entry => entry.Section == section)
            .Sum(GetEntryYearlyTotal);
    }
}