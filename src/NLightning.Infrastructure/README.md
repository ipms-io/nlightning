# NLightning.Infrastructure

NLightning.Infrastructure is a core component of the NLightning ecosystem, providing essential infrastructure services for Lightning Network implementations in .NET. This library serves as a bridge between the application and domain layers and external dependencies.

## Features

- Cross-platform cryptographic operations with support for:
    - Native environments (AOT/Native) using BouncyCastle and Argon2
    - Server/desktop environments using `libsodium`
    - Blazor WebAssembly using JavaScript interop and `libsodium.js`
- Bitcoin and Lightning Network integration via NBitcoin
- DNS resolution capabilities
- Comprehensive logging support

## Available Packages

We've decided to have 2 packages, one for desktop/server development, and one for Blazor WebAssembly development.

The reason behind this is that for a Blazor app running fully on the browser we don't have access to native libsodium.

## Installation

Install the package from NuGet:

```bash
# For the "standard" version of the package run
dotnet add package NLightning.Infrastructure

# For the Blazor WebAssembly version run
dotnet add package NLightning.Infrastructure.Blazor
```

## Configuration

NLightning.Infrastructure automatically adapts to your build environment:

- For AOT/Native builds, it uses BouncyCastle and Argon2
- For standard .NET applications, it uses libsodium
- For Blazor WebAssembly, it provides JavaScript interoperability

### Blazor WebAssembly Setup

When using in a Blazor WebAssembly project, you need to initialize the crypto provider:

```csharp
using NLightning.Infrastructure.Crypto.Providers.JS;

// Initialize before starting the app
await BlazorCryptoProvider.InitializeBlazorCryptoProviderAsync();

await builder.Build().RunAsync();
```

## Usage

NLightning.Infrastructure is primarily used by other NLightning components, but you can also use it directly for:

```csharp
// Crypto operations, networking, and other infrastructure services
// Detailed documentation coming soon
```

## Dependencies

- NBitcoin and NBitcoin.Secp256k1 for Bitcoin operations
- Cryptographic libraries (BouncyCastle, Argon2, or libsodium depending on environment)
- DnsClient for DNS resolution
- Microsoft.Extensions.Logging for logging infrastructure

## Related Projects

- NLightning.Application
- NLightning.Common
- NLightning.Domain
- NLightning.Bolt11