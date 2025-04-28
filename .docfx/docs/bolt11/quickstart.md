# NLightning.Bolt11

This library provides a decoder/encoder for the BOLT11 invoice format used in the Lightning Network.
It can be used to decode and/or encode BOLT11 invoices.

## Available Packages

We've decided to have 2 packages, one for desktop/server development, and one for Blazor WebAssembly development.

The reason behind this is that for a Blazor app running fully on the browser we don't have access to native libsodium.

### WebAssembly Instructions

For WebAssembly instructions click [here](bolt11/webassembly)

## Usage

Follow the steps below to install and decode bolt11 invoices.

### Installation

Install the package from NuGet:

```bash
dotnet add package NLightning.Bolt11
```

### Decoding

```csharp
// add the using directive
using NLightning.Bolts.BOLT11;

// decode the invoice string
var invoice = Invoice.Decode(invoice_string);

// Get properties of the invoice
Console.WriteLine("Here's a few props from the invoice:")
Console.WriteLine(invoice.AmountMillisats);
Console.WriteLine(invoice.AmountSats);
Console.WriteLine(invoice.PaymentHash);
Console.WriteLine("A list with all the props can be found at: https://nlightning.ipms.io/api/NLightning.Bolts.BOLT11.Invoice.html#properties");
```

### Configuration
Before working with BOLT11 invoices, you might need to set up some configuration values.
Two important static helper classes are used in this process:

#### SecureKeyManager
This class is responsible for securely managing cryptographic keys.
It needs to be initialized before using any functionality that relies on key security.

```csharp
// Initialize SecureKeyManager with the virtual node key
SecureKeyManager.Initialize(virtualNodeKey.ToBytes());
```