using System.Text;

namespace NLightning.Bolts.BOLT11.Encoders;

using BOLT11.Constants;
using NBitcoin.DataEncoders;

public sealed class Bech32Encoder(string? hrp = null) : NBitcoin.DataEncoders.Bech32Encoder(hrp is null ? Encoding.UTF8.GetBytes(InvoiceConstants.PREFIX) : Encoding.UTF8.GetBytes(hrp))
{
    public string EncodeLightningInvoice(byte[] data, byte[] signature)
    {
        var invoiceData = new byte[data.Length + signature.Length];
        data.CopyTo(invoiceData, 0);
        signature.CopyTo(invoiceData, data.Length);

        // Convert to 5 bits per byte
        var convertedData = ConvertBits(invoiceData.AsReadOnly(), 8, 5);

        return EncodeData(convertedData, Bech32EncodingType.BECH32M);
    }

    public static void DecodeLightningInvoice(string invoiceString, out byte[] data, out byte[] signature, out string hrp)
    {
        // Be lenient and coverti it all to lower case
        invoiceString = invoiceString.ToLowerInvariant();

        // Validate the prefix
        if (!invoiceString.StartsWith(InvoiceConstants.PREFIX))
        {
            throw new ArgumentException("Missing prefix in invoice", nameof(invoiceString));
        }

        // Extract human readable part
        var separatorIndex = invoiceString.LastIndexOf(InvoiceConstants.SEPARATOR);
        if (separatorIndex == -1)
        {
            throw new ArgumentException("Invalid invoice format", nameof(invoiceString));
        }
        else if (separatorIndex == 0)
        {
            throw new ArgumentException("Missing human readable part in invoice", nameof(invoiceString));
        }
        else if (separatorIndex > 21)
        {
            throw new ArgumentException("Human readable part too long in invoice", nameof(invoiceString));
        }

        hrp = invoiceString[..separatorIndex];
        var bech32 = new Bech32Encoder(invoiceString[..separatorIndex])
        {
            StrictLength = false
        };
        var invoiceData = bech32.DecodeDataRaw(invoiceString, out _);

        signature = bech32.ConvertBits(invoiceData.AsSpan()[(invoiceData.Length - 104)..], 5, 8);
        data = bech32.ConvertBits(invoiceData.AsSpan()[..(invoiceData.Length - 104)], 5, 8);
    }

    public static void ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, out byte[] newData)
    {
        newData = new Bech32Encoder().ConvertBits(data, fromBits, toBits);
    }
}