using B4BudgetCore.Models;

namespace B4BudgetCore.Services;

public interface ICalculationService
{
    decimal GetEntryMonthValue(BudgetEntry entry, int monthOffset);
    decimal GetMonthTotal(Budget budget, SectionType section, int monthOffset);
    decimal GetMonthSurplus(Budget budget, int monthOffset);
    decimal GetRunningBalance(Budget budget, int monthOffset);
    decimal GetYearEndBalance(Budget budget);
    decimal GetEntryYearlyTotal(BudgetEntry entry);
    decimal GetSectionYearlyTotal(Budget budget, SectionType section);
}