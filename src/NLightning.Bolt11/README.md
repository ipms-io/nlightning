# NLightning.Bolt11

This library provides a decoder/encoder for the BOLT11 invoice format used in the Lightning Network.
It can be used to decode and/or encode BOLT11 invoices.

## Available Packages

We've decided to have 2 packages, one for desktop/server development, and one for Blazor WebAssembly development.

The reason behind this is that for a Blazor app running fully on the browser we don't have access to native libsodium.

## Sample

A sample project using this project in a Blazor WebAssembly environment can be found [here](https://github.com/ipms-io/NLightning-Samples/tree/main/Bolt11.Decoder.Blazor).

A live version of the sample can be found at [bolt11.ipms.io](https://bolt11.ipms.io)

## Usage

Follow the steps below to install and decode bolt11 invoices.

### Installation

Install the package from NuGet:

```bash
# For the "regular" version of the package run
dotnet add package NLightning.Bolt11

# For the Blazor WebAssembly version run
dotnet add package NLightning.Bolt11.Blazor
```

### Decoding

```csharp
// add the using directive
using NLightning.Bolts.BOLT11;

// decode the invoice string
var invoice = Invoice.Decode(invoice_string);

// Get properties of the invoice
Console.WriteLine("Here's a few props from the invoice:")
Console.WriteLine(invoice.Amount.MilliSatoshi);
Console.WriteLine(invoice.Amount.Satoshi);
Console.WriteLine(invoice.PaymentHash);
Console.WriteLine("A list with all the props can be found at: https://nlightning.ipms.io/api/NLightning.Bolts.BOLT11.Invoice.html#properties");
```

### Decoding in Blazor Apps

Blazor apps need to initialize the CryptoProvider in order to load the needed js files. Add the following to your
`Program.cs` file.

#### Initialize the CryptoProvider (libsodium.js)

```csharp
// Add the using directive
using NLightning.Infrastructure.Crypto.Providers.JS;

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
    <p>Amount MilliSats: @invoice.Amount.MilliSatoshi</p>
    <p>Amount Sats: @invoice.Amount.Satoshi</p>
    <p>Amount BTC: @invoice.Amount</p>
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

