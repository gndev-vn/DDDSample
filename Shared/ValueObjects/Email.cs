namespace Shared.ValueObjects;

public sealed class Email
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (!value.Contains('@'))
        {
            throw new ArgumentException("Invalid email");
        }

        Value = value;
    }
}
