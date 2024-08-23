using System.Text;
using NBitcoin.DataEncoders;

namespace NLightning.Bolts.BOLT11.Encoders;

using Common.BitUtils;
using Constants;

/// <summary>
/// Bech32 encoder for lightning invoices
/// </summary>
/// <param name="hrp">The Human Readable Part</param>
internal sealed class Bech32Encoder(string? hrp = null) : NBitcoin.DataEncoders.Bech32Encoder(hrp is null ? Encoding.UTF8.GetBytes(InvoiceConstants.PREFIX) : Encoding.UTF8.GetBytes(hrp))
{
    /// <summary>
    /// Encode the lightning invoice into a bech32 string
    /// </summary>
    /// <param name="bitWriter">Bit writer to write to</param>
    /// <param name="signature">Signature to encode</param>
    /// <returns>String representing the invoice</returns>
    internal string EncodeLightningInvoice(BitWriter bitWriter, byte[] signature)
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

    /// <summary>
    /// Decode the lightning invoice from a bech32 string
    /// </summary>
    /// <param name="invoiceString">String representing the invoice</param>
    /// <param name="data">Data part of the invoice</param>
    /// <param name="signature">Signature part of the invoice</param>
    /// <param name="hrp">Human Readable Part of the invoice</param>
    internal static void DecodeLightningInvoice(string invoiceString, out byte[] data, out byte[] signature, out string hrp)
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
}