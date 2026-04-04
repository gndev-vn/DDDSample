namespace OrderingAPI.Features.Customers.CreateCustomer;

public sealed class CreateCustomerRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
