namespace B4BudgetCore.Models;

public class BudgetEntry
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SectionType Section { get; set; }
    public RecurrenceType Recurrence { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Budget Budget { get; set; } = null!;
    public ICollection<MonthValue> MonthValues { get; set; } = new List<MonthValue>();
}