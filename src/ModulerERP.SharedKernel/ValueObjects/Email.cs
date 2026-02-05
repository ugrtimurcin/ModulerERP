namespace ModulerERP.SharedKernel.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email);
    }

    public static Email? TryCreate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        try
        {
            return Create(email);
        }
        catch
        {
            return null;
        }
    }

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public static bool operator ==(Email? left, Email? right) => Equals(left, right);
    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);
    public static implicit operator string(Email email) => email.Value;
}
