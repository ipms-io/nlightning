using System.Globalization;

namespace NLightning.Common.Formatters;

using Enums;

public class LightningFormatter : IFormatProvider, ICustomFormatter
{
    public static readonly LightningFormatter FORMATTER = new LightningFormatter();

    public object? GetFormat(Type formatType)
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
        if (int.TryParse(format.AsSpan(i, 1), out var decPos))
        {
            i++;
        }
        var unit = format[i];
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