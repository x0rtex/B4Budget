namespace B4Budget.Models;

public class Budget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartMonth { get; set; } = 1;
    public int StartYear { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();
}