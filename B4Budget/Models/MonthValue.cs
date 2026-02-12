namespace B4Budget.Models;

public class MonthValue
{
    public int Id { get; set; }
    public int BudgetItemId { get; set; }
    public int MonthOffset { get; set; }
    public decimal Value { get; set; }
    public bool IsOverride { get; set; }
    public BudgetItem BudgetItem { get; set; } = null!;
}