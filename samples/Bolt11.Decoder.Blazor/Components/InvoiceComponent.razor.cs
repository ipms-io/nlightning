using Microsoft.AspNetCore.Components;
using NLightning.Bolts.BOLT11;

namespace Bolt11.Decoder.Blazor.Components;

public partial class InvoiceComponent
{
    [Parameter]
    public Invoice? Invoice { get; set; }

    private IEnumerable<InvoiceItem>? _invoiceItems;

    protected override void OnInitialized()
    {
        if (Invoice == null) return;

        List<InvoiceItem> invoiceItems =
        [
            new InvoiceItem("Human Readable Part", "(hrp)", Invoice.HumanReadablePart),
            new InvoiceItem("Network", "", Invoice.Network),
            new InvoiceItem("Amount MilliSatoshis", "", Invoice.AmountMilliSats.ToString()),
            new InvoiceItem("Amount Satoshis", "", Invoice.AmountSats.ToString()),
            new InvoiceItem("Timestamp", "", $"{Invoice.Timestamp.ToString()} | {DateTimeOffset.FromUnixTimeSeconds(Invoice.Timestamp).LocalDateTime:yyyy-MM-d HH:mm:ss zz}"),
            new InvoiceItem("Signature", "", Convert.ToHexString(Invoice.Signature.Signature)),
            new InvoiceItem("Recovery Flag", "", Invoice.Signature.RecoveryId.ToString()),
            new InvoiceItem("Payment Hash", "(p)", Invoice.PaymentHash.ToString())
        ];

        // Check for routing info
        if (Invoice.RoutingInfos is { Count: > 0 })
        {
            foreach (var routingInfo in Invoice.RoutingInfos)
            {
                invoiceItems.Add(new InvoiceItem("Routing Info", "(r)", ""));
                invoiceItems.Add(new InvoiceItem("\u21b3", "Public Key", routingInfo.PubKey.ToString()));
                invoiceItems.Add(new InvoiceItem("\u21b3", "Short Channel Id", routingInfo.ShortChannelId.ToString()));
                invoiceItems.Add(new InvoiceItem("\u21b3", "Fee Base (MSats)", routingInfo.FeeBaseMsat.ToString()));
                invoiceItems.Add(new InvoiceItem("\u21b3", "Fee Proportional (millionths)", routingInfo.FeeProportionalMillionths.ToString()));
                invoiceItems.Add(new InvoiceItem("\u21b3", "CLTV Expiry Delta", routingInfo.CltvExpiryDelta.ToString()));
            }
        }

        // Check for features
        if (Invoice.Features is not null)
        {
            invoiceItems.Add(new InvoiceItem("Features", "(9)", Invoice.Features.ToString()));
        }

        // Add expiry time
        invoiceItems.Add(new InvoiceItem("Expiry Time", "(x)", Invoice.ExpiryDate.LocalDateTime.ToString("yyyy-MM-d HH:mm:ss zz")));

        // Check for fallback address
        if (Invoice.FallbackAddresses is { Count: > 0 })
        {
            invoiceItems.Add(new InvoiceItem("Fallback Address", "(f)", ""));
            invoiceItems.AddRange(Invoice.FallbackAddresses.Select(fallbackAddress => new InvoiceItem("\u21b3", "Address", fallbackAddress.ToString())));
        }

        // Check for description
        if (Invoice.Description is not null)
        {
            invoiceItems.Add(new InvoiceItem("Description", "(d)", Invoice.Description));
        }

        // Add payment secret
        invoiceItems.Add(new InvoiceItem("Payment Secret", "(s)", Invoice.PaymentSecret.ToString()));

        // Check for payee pub key
        if (Invoice.PayeePubKey is not null)
        {
            invoiceItems.Add(new InvoiceItem("Payee PubKey", "(n)", Invoice.PayeePubKey.ToString()));
        }

        // Check for description hash
        if (Invoice.DescriptionHash is not null)
        {
            invoiceItems.Add(new InvoiceItem("Description Hash", "(h)", Invoice.DescriptionHash.ToString()));
        }

        // Check for min final cltv expiry
        if (Invoice.MinFinalCltvExpiry is not null)
        {
            invoiceItems.Add(new InvoiceItem("Min Final CLTV Expiry", "(c)", Invoice.MinFinalCltvExpiry.ToString()!));
        }

        // Check for metadata
        if (Invoice.Metadata is not null)
        {
            invoiceItems.Add(new InvoiceItem("Metadata", "(m)", Convert.ToHexString(Invoice.Metadata)));
        }

        _invoiceItems = invoiceItems.AsQueryable();
    }
}

internal sealed class InvoiceItem(string name, string subName, string value)
{
    public string Name { get; } = name;
    public string SubName { get; } = subName;
    public string Value { get; } = value;
}