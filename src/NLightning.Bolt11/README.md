# NLightning.Bolt11

This library provides a decoder/encoder for the BOLT11 invoice format used in the Lightning Network.
It can be used to decode and/or encode BOLT11 invoices.

## Usage

Install the package from NuGet:

```
dotnet add package NLightning.Bolt11
```

### Decoding

```csharp
// add the using directive
using NLightning.Bolt11;

// decode the invoice string
var invoice = Invoice.Decode(invoice_string);

// Get properties of the invoice
Console.WriteLine(invoice.Amount);
Console.WriteLine(invoice.PaymentHash);
```