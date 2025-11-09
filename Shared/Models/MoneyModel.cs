namespace Shared.Models;

public class MoneyModel : ModelBase
{
    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;
}