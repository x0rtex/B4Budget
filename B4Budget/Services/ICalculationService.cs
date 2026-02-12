using B4Budget.Models;

namespace B4Budget.Services;

public interface ICalculationService
{
    decimal GetItemMonthValue(BudgetItem item, int monthOffset);
    decimal GetMonthTotal(Budget budget, SectionType section, int monthOffset);
    decimal GetMonthNet(Budget budget, int monthOffset);
    decimal GetRunningBalance(Budget budget, int monthOffset);
    decimal GetYearEndBalance(Budget budget);
    decimal GetItemYearlyTotal(BudgetItem item);
    decimal GetSectionYearlyTotal(Budget budget, SectionType section);
}