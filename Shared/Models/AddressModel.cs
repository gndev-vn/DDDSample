namespace Shared.Models;

public class AddressModel(string line1, string? line2, string city, string province, string district, string ward)
    : ModelBase
{
    public string Line1 { get; set; } = line1;

    public string? Line2 { get; set; } = line2;

    public string City { get; set; } = city;

    public string Province { get; set; } = province;

    public string District { get; set; } = district;

    public string Ward { get; set; } = ward;
}