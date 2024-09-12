# Web Assembly / Blazor

This library provides a decoder/encoder for the BOLT11 invoice format used in the Lightning Network.
It can be used to decode and/or encode BOLT11 invoices in a WebAssembly app.

## Usage

Follow the steps below to install and decode bolt11 invoices.

### Installation

Install the package from NuGet:

```bash
dotnet add package NLightning.Bolt11.Blazor
```

### Decoding

Blazor apps need to initialize the CryptoProvider in order to load the needed js files. Add the following to your
`Program.cs` file.

#### Initialize the CryptoProvider (libsodium.js)

```csharp
// Add the using directive
using NLightning.Common.Crypto.Providers.JS;

// ...
// Your app code

// Initialize the Crypto Provider just before starting the server.
await BlazorCryptoProvider.InitializeBlazorCryptoProviderAsync();

await builder.Build().RunAsync();
```

#### Decode the invoice

```blazor
<button @onclick="DecodeInvoice">Decode</button>
<br/>
@if (invoice != null)
{
    <p>Payment Hash: @invoice.PaymentHash</p>
    <p>Amount MilliSats: @invoice.AmountMilliSats</p>
    <p>Amount Sats: @invoice.AmountSats</p>
    <p>Description: @invoice.Description</p>
}

@code{

    Invoice? invoice;
    
    void DecodeInvoice()
    {
        invoice = Invoice.Decode("lnbc1pndpjfppp5qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqssp5qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqsdqq5243a4h29w7lm6g89hktd0qzfakevjp7hktskal5p69jxa6vyqw4s95577lltw0t6l9dhp7cfld9urkxfsucsxascnxdqmanrlklsqcp5nwzmf");
    }

}
```