using System.Globalization;

namespace NLightning.Common.Formatters;

using Enums;

public class LightningFormatter : IFormatProvider, ICustomFormatter
{
    public static readonly LightningFormatter FORMATTER = new();

    public object? GetFormat(Type? formatType)
    {
        return formatType == typeof(ICustomFormatter) ? this : null;
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (!Equals(formatProvider) || string.IsNullOrWhiteSpace(format))
        {
            return string.Empty;
        }

        var i = 0;

        // Parse all leading digits for the decimal position
        var decimalPositionSpan = format.AsSpan(i);
        var decPos = 0;
        while (i < decimalPositionSpan.Length && char.IsDigit(decimalPositionSpan[i]))
        {
            decPos = decPos * 10 + (decimalPositionSpan[i] - '0');
            i++;
        }
        var unit = decimalPositionSpan[i];
        var unitToUseInCalc = unit switch
        {
            'B' => LightningMoneyUnit.BTC,
            _ => LightningMoneyUnit.BTC
        };
        var val = Convert.ToDecimal(arg, CultureInfo.InvariantCulture) / (ulong)unitToUseInCalc;
        var zeros = new string('0', decPos);
        var rest = new string('#', 11 - decPos);

        var fmt = "{0:0" + (decPos > 0 ? "." + zeros + rest : string.Empty) + "}";
        return string.Format(CultureInfo.InvariantCulture, fmt, val);
    }
}