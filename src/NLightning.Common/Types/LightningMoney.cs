using System.Globalization;
using NBitcoin;

namespace NLightning.Common.Types;

using Enums;
using Formatters;

public class LightningMoney : IMoney
{
    // for decimal.TryParse. None of the NumberStyles' composed values is useful for bitcoin style
    private const NumberStyles BITCOIN_STYLE = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                                              | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    private ulong _milliSatoshi;

    public const ulong COIN = 100 * 1000 * 1000 * 1000UL;
    public const ulong CENT = COIN / 100;
    public const ulong NANO = CENT / 100;

    public ulong MilliSatoshi
    {
        get => _milliSatoshi;
        set
        {
            _milliSatoshi = value;
        }
    }

    public long Satoshi
    {
        get => checked((long)(_milliSatoshi / 1000));
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Satoshi value cannot be negative");

            checked
            {
                _milliSatoshi = (ulong)(value * 1000);
            }
        }
    }

    public static LightningMoney Zero => 0UL;
    public bool IsZero => _milliSatoshi == 0;

    #region Constructors
    public LightningMoney(ulong milliSatoshi)
    {
        MilliSatoshi = milliSatoshi;
    }

    public LightningMoney(decimal amount, LightningMoneyUnit unit)
    {
        // sanity check. Only valid units are allowed
        CheckLightningMoneyUnit(unit, nameof(unit));
        checked
        {
            var milliSats = amount * (long)unit;
            MilliSatoshi = (ulong)milliSats;
        }
    }

    public LightningMoney(long amount, LightningMoneyUnit unit)
    {
        // sanity check. Only valid units are allowed
        CheckLightningMoneyUnit(unit, nameof(unit));
        checked
        {
            var milliSats = amount * (long)unit;
            MilliSatoshi = (ulong)milliSats;
        }
    }

    public LightningMoney(ulong amount, LightningMoneyUnit unit)
    {
        // sanity check. Only valid units are allowed
        CheckLightningMoneyUnit(unit, nameof(unit));
        checked
        {
            var milliSats = amount * (ulong)unit;
            MilliSatoshi = milliSats;
        }
    }
    #endregion

    #region Parsers
    /// <summary>
    /// Parse a bitcoin amount (Culture Invariant)
    /// </summary>
    /// <param name="bitcoin"></param>
    /// <param name="nRet"></param>
    /// <returns></returns>
    public static bool TryParse(string bitcoin, out LightningMoney? nRet)
    {
        nRet = null;

        if (!decimal.TryParse(bitcoin, BITCOIN_STYLE, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        try
        {
            nRet = new LightningMoney(value, LightningMoneyUnit.BTC);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Parse a bitcoin amount (Culture Invariant)
    /// </summary>
    /// <param name="bitcoin"></param>
    /// <returns></returns>
    public static LightningMoney? Parse(string bitcoin)
    {
        if (TryParse(bitcoin, out var result))
        {
            return result;
        }
        throw new FormatException("Impossible to parse the string in a bitcoin amount");
    }
    #endregion

    #region Conversions
    /// <summary>
    /// Convert Money to decimal (same as ToDecimal)
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public decimal ToUnit(LightningMoneyUnit unit)
    {
        CheckLightningMoneyUnit(unit, nameof(unit));
        // overflow safe because (long / int) always fit in decimal
        // decimal operations are checked by default
        return (decimal)MilliSatoshi / (int)unit;
    }
    /// <summary>
    /// Convert Money to decimal (same as ToUnit)
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public decimal ToDecimal(LightningMoneyUnit unit)
    {
        return ToUnit(unit);
    }
    #endregion

    /// <summary>
    /// Split the Money in parts without loss
    /// </summary>
    /// <param name="parts">The number of parts (must be more than 0)</param>
    /// <returns>The splitted money</returns>
    public IEnumerable<LightningMoney> Split(int parts)
    {
        if (parts <= 0)
            throw new ArgumentOutOfRangeException(nameof(parts), "Parts should be more than 0");

        var result = DivRem(_milliSatoshi, (ulong)parts, out var remain);

        for (var i = 0; i < parts; i++)
        {
            yield return LightningMoney.MilliSatoshis(result + (remain > 0 ? 1UL : 0UL));
            remain--;
        }
    }
    IEnumerable<IMoney> IMoney.Split(int parts)
    {
        return Split(parts);
    }

    #region Static Converters
    public static LightningMoney FromUnit(decimal amount, LightningMoneyUnit unit)
    {
        return new LightningMoney(amount, unit);
    }

    public static LightningMoney Coins(decimal coins)
    {
        // overflow safe.
        // decimal operations are checked by default
        return new LightningMoney(coins * COIN, LightningMoneyUnit.MILLI_SATOSHI);
    }

    public static LightningMoney Bits(decimal bits)
    {
        // overflow safe.
        // decimal operations are checked by default
        return new LightningMoney(bits * CENT, LightningMoneyUnit.MILLI_SATOSHI);
    }

    public static LightningMoney Cents(decimal cents)
    {
        // overflow safe.
        // decimal operations are checked by default
        return new LightningMoney(cents * CENT, LightningMoneyUnit.MILLI_SATOSHI);
    }

    public static LightningMoney Satoshis(decimal sats)
    {
        return new LightningMoney(sats, LightningMoneyUnit.MILLI_SATOSHI);
    }

    public static LightningMoney Satoshis(ulong sats)
    {
        return new LightningMoney(sats);
    }

    public static LightningMoney Satoshis(long sats)
    {
        return new LightningMoney((ulong)sats);
    }

    public static LightningMoney MilliSatoshis(ulong milliSats)
    {
        return new LightningMoney(milliSats);
    }

    public static LightningMoney MilliSatoshis(long sats)
    {
        return new LightningMoney((ulong)sats);
    }
    #endregion

    #region IEquatable<Money> Members
    public bool Equals(LightningMoney? other)
    {
        return other is not null && _milliSatoshi.Equals(other._milliSatoshi);
    }
    bool IEquatable<IMoney>.Equals(IMoney? other)
    {
        return Equals(other);
    }

    public int CompareTo(LightningMoney? other)
    {
        return other is null ? 1 : _milliSatoshi.CompareTo(other._milliSatoshi);
    }

    bool IMoney.IsCompatible(IMoney money)
    {
        ArgumentNullException.ThrowIfNull(money);

        return money is LightningMoney;
    }
    #endregion

    #region IComparable Members
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            LightningMoney m => _milliSatoshi.CompareTo(m._milliSatoshi),
            _ => _milliSatoshi.CompareTo((ulong)obj)
        };
    }
    int IComparable.CompareTo(object? obj)
    {
        return CompareTo(obj);
    }
    int IComparable<IMoney>.CompareTo(IMoney? other)
    {
        return CompareTo(other);
    }
    #endregion

    #region Unary Operators
    public static LightningMoney operator -(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new LightningMoney(checked(left._milliSatoshi - right._milliSatoshi));
    }
    public static LightningMoney operator -(LightningMoney _)
    {
        throw new ArithmeticException("LightningMoney does not support unary negation");
    }
    public static LightningMoney operator +(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new LightningMoney(checked(left._milliSatoshi + right._milliSatoshi));
    }
    public static LightningMoney operator *(ulong left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(right);

        return MilliSatoshis(checked(left * right._milliSatoshi));
    }

    public static LightningMoney operator *(LightningMoney left, ulong right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return MilliSatoshis(checked(left._milliSatoshi * right));
    }
    public static LightningMoney operator *(long left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(right);

        return MilliSatoshis(checked((ulong)left * right._milliSatoshi));
    }
    public static LightningMoney operator *(LightningMoney left, long right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return MilliSatoshis(checked((ulong)right * left._milliSatoshi));
    }

    public static LightningMoney operator /(LightningMoney left, ulong right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return new LightningMoney(left._milliSatoshi / right);
    }

    public static bool operator <(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left._milliSatoshi < right._milliSatoshi;
    }
    public static bool operator >(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left._milliSatoshi > right._milliSatoshi;
    }
    public static bool operator <=(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left._milliSatoshi <= right._milliSatoshi;
    }
    public static bool operator >=(LightningMoney left, LightningMoney right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left._milliSatoshi >= right._milliSatoshi;
    }
    #endregion

    #region Implicit Operators
    public static implicit operator LightningMoney(long value)
    {
        return new LightningMoney((ulong)value);
    }
    public static implicit operator LightningMoney(ulong value)
    {
        return new LightningMoney(value);
    }
    public static implicit operator LightningMoney(Money value)
    {
        return new LightningMoney((ulong)value.Satoshi * 1_000UL);
    }
    public static implicit operator LightningMoney(string value)
    {
        return Parse(value) ?? throw new ArgumentException("Cannot parse value into a valid LightningMoney", nameof(value));
    }

    public static implicit operator long(LightningMoney value)
    {
        return checked((long)value.MilliSatoshi);
    }

    public static implicit operator ulong(LightningMoney value)
    {
        return value.MilliSatoshi;
    }

    public static implicit operator Money(LightningMoney value)
    {
        return new Money(value.Satoshi);
    }
    #endregion

    #region Equality Operators
    public override bool Equals(object? obj)
    {
        return obj is LightningMoney item && _milliSatoshi.Equals(item._milliSatoshi);
    }
    public static bool operator ==(LightningMoney? a, LightningMoney? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return a._milliSatoshi == b._milliSatoshi;
    }

    public static bool operator !=(LightningMoney a, LightningMoney b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return _milliSatoshi.GetHashCode();
    }
    #endregion

    #region ToString
    /// <summary>
    /// Returns a culture invariant string representation of Bitcoin amount
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ToString(false);
    }
    /// <summary>
    /// Returns a culture invariant string representation of Bitcoin amount
    /// </summary>
    /// <param name="trimExcessZero">True if trim excess zeros</param>
    /// <returns></returns>
    public string ToString(bool trimExcessZero = true)
    {
        var fmt = string.Format(CultureInfo.InvariantCulture, "{{0:{0}B}}", trimExcessZero ? "2" : "11");
        return string.Format(LightningFormatter.FORMATTER, fmt, _milliSatoshi);
    }
    #endregion

    /// <summary>
    /// Tell if amount is almost equal to this instance
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="dust">more or less amount</param>
    /// <returns>true if equals, else false</returns>
    public bool Almost(LightningMoney amount, LightningMoney dust)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(dust);

        return checked(amount - this) <= dust;
    }

    /// <summary>
    /// Tell if amount is almost equal to this instance
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="margin">error margin (between 0 and 1)</param>
    /// <returns>true if equals, else false</returns>
    public bool Almost(LightningMoney amount, decimal margin)
    {
        ArgumentNullException.ThrowIfNull(amount);
        if (margin is < 0.0m or > 1.0m)
            throw new ArgumentOutOfRangeException(nameof(margin), "margin should be between 0 and 1");

        var dust = Satoshis(MilliSatoshi * margin);
        return Almost(amount, dust);
    }

    public static LightningMoney Min(LightningMoney a, LightningMoney b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return a <= b ? a : b;
    }

    public static LightningMoney Max(LightningMoney a, LightningMoney b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return a >= b ? a : b;
    }

    #region IMoney Members
    public IMoney Add(IMoney money)
    {
        return this + (LightningMoney)money;
    }

    public IMoney Sub(IMoney money)
    {
        return this - (LightningMoney)money;
    }

    public IMoney Negate()
    {
        throw new ArithmeticException("LightningMoney does not support unary negation");
    }
    #endregion

    private static void CheckLightningMoneyUnit(LightningMoneyUnit value, string paramName)
    {
        var typeOfMoneyUnit = typeof(LightningMoneyUnit);
        if (!Enum.IsDefined(typeOfMoneyUnit, value))
        {
            throw new ArgumentException("Invalid value for LightningMoneyUnit", paramName);
        }
    }

    private static ulong DivRem(ulong a, ulong b, out ulong result)
    {
        result = a % b;
        return a / b;
    }
}