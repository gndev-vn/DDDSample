namespace OrderingAPI.Features.Customers.UpdateCustomer;

public sealed class UpdateCustomerRequest
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}
