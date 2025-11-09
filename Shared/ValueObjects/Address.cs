using Shared.Models;

namespace Shared.ValueObjects;

public sealed class Address : ValueObject
{
    public string Line1 { get; }

    public string? Line2 { get; }

    public string City { get; }

    public string Province { get; }

    public string District { get; }

    public string Ward { get; }

    public Address(string line1, string? line2, string ward, string district, string city, string province)
    {
        if (string.IsNullOrWhiteSpace(line1))
        {
            throw new ArgumentException("line1 required");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("city required");
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            throw new ArgumentException("province required");
        }

        if (string.IsNullOrWhiteSpace(district))
        {
            throw new ArgumentException("district required");
        }

        if (string.IsNullOrWhiteSpace(ward))
        {
            throw new ArgumentException("ward required");
        }

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        Ward = ward.Trim();
        District = district.Trim();
        City = city.Trim();
        Province = province.Trim();
    }

    public override string ToString() => $"{Line1}, {Line2}, {Ward}, {District}, {City}, {Province}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1.ToLowerInvariant();
        yield return Line2?.ToLowerInvariant();
        yield return Ward.ToLowerInvariant();
        yield return District.ToLowerInvariant();
        yield return City.ToLowerInvariant();
        yield return Province.ToLowerInvariant();
    }
}