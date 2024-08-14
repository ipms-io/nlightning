using System.Text;
using NLightning.Common.BitUtils;

namespace NLightning.Bolts.BOLT11.Encoders;

using Constants;
using NBitcoin.DataEncoders;

public sealed class Bech32Encoder(string? hrp = null) : NBitcoin.DataEncoders.Bech32Encoder(hrp is null ? Encoding.UTF8.GetBytes(InvoiceConstants.PREFIX) : Encoding.UTF8.GetBytes(hrp))
{
    public string EncodeLightningInvoice(BitWriter bitWriter, byte[] signature)
    {
        // Convert to 5 bits per byte
        var convertedSignature = ConvertBits(signature.AsReadOnly(), 8, 5);
        var convertedData = ConvertBits(bitWriter.ToArray().AsReadOnly(), 8, 5);

        // Check for padding
        var expectedLength = bitWriter.TotalBits / 5;
        if (convertedData.Length != expectedLength)
        {
            convertedData = convertedData[..expectedLength];
        }

        var invoiceData = new byte[104 + convertedData.Length];
        convertedData.CopyTo(invoiceData, 0);
        convertedSignature.CopyTo(invoiceData, convertedData.Length);

        return EncodeData(invoiceData, Bech32EncodingType.BECH32);
    }

    public static void DecodeLightningInvoice(string invoiceString, out byte[] data, out byte[] signature, out string hrp)
    {
        // Be lenient and covert it all to lower case
        invoiceString = invoiceString.ToLowerInvariant();

        // Validate the prefix
        if (!invoiceString.StartsWith(InvoiceConstants.PREFIX))
        {
            throw new ArgumentException("Missing prefix in invoice", nameof(invoiceString));
        }

        // Extract human readable part
        var separatorIndex = invoiceString.LastIndexOf(InvoiceConstants.SEPARATOR);
        switch (separatorIndex)
        {
            case -1:
                throw new ArgumentException("Invalid invoice format", nameof(invoiceString));
            case 0:
                throw new ArgumentException("Missing human readable part in invoice", nameof(invoiceString));
            case > 21:
                throw new ArgumentException("Human readable part too long in invoice", nameof(invoiceString));
        }

        hrp = invoiceString[..separatorIndex];
        var bech32 = new Bech32Encoder(invoiceString[..separatorIndex])
        {
            StrictLength = false
        };
        var invoiceData = bech32.DecodeDataRaw(invoiceString, out var encodingType);

        if (encodingType != Bech32EncodingType.BECH32)
        {
            throw new ArgumentException("Invalid encoding type in invoice", nameof(invoiceString));
        }

        signature = bech32.ConvertBits(invoiceData.AsSpan()[(invoiceData.Length - 104)..], 5, 8);
        data = bech32.ConvertBits(invoiceData.AsSpan()[..(invoiceData.Length - 104)], 5, 8);
    }

    public static void ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, out byte[] newData)
    {
        newData = new Bech32Encoder().ConvertBits(data, fromBits, toBits);
    }
}