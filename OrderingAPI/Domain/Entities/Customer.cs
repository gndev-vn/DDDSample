using Shared.Models;

namespace OrderingAPI.Domain.Entities;

public class Customer : Entity
{
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Customer()
    {
    }

    public static Customer Create(string displayName, string email, string? phoneNumber, bool isActive, string? address = null)
        => Create(Guid.NewGuid(), displayName, email, phoneNumber, isActive, address);

    public static Customer Create(Guid id, string displayName, string email, string? phoneNumber, bool isActive, string? address = null)
    {
        var customer = new Customer { Id = id };
        customer.Update(displayName, email, phoneNumber, isActive, address);
        return customer;
    }

    public void Update(string displayName, string email, string? phoneNumber, bool isActive, string? address = null)
    {
        DisplayName = NormalizeRequired(displayName, nameof(displayName));
        Email = NormalizeRequired(email, nameof(email));
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
