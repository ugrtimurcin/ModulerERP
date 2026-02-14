namespace ModulerERP.SharedKernel.ValueObjects;

/// <summary>
/// Money value object for handling currency amounts with precision
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; }

    private Money() { }

    private Money(decimal amount, string currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public static Money Create(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required", nameof(currencyCode));

        return new Money(Math.Round(amount, 2), currencyCode.ToUpperInvariant());
    }

    public static Money Zero(string currencyCode) => Create(0, currencyCode);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, CurrencyCode);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, CurrencyCode);
    }

    public Money Multiply(decimal factor)
        => Create(Amount * factor, CurrencyCode);

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero");
        return Create(Amount / divisor, CurrencyCode);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException($"Cannot perform operation on different currencies: {CurrencyCode} and {other.CurrencyCode}");
    }

    public override string ToString() => $"{Amount:N2} {CurrencyCode}";
    public override int GetHashCode() => HashCode.Combine(Amount, CurrencyCode);
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public bool Equals(Money? other) => other is not null && Amount == other.Amount && CurrencyCode == other.CurrencyCode;
    public static bool operator ==(Money? left, Money? right) => Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal factor) => left.Multiply(factor);
    public static Money operator /(Money left, decimal divisor) => left.Divide(divisor);
}
