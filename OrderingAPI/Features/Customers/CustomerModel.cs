using Shared.Models;

namespace OrderingAPI.Features.Customers;

public class CustomerModel : ModelBase
{
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; }
    public int OrderCount { get; init; }
}
