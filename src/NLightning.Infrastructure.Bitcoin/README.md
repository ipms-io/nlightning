# NLightning.Infrastructure.Bitcoin

`NLightning.Infrastructure.Bitcoin` is a dedicated component within the NLightning ecosystem, providing essential
Bitcoin-specific infrastructure services for Lightning Network implementations in .NET. This library focuses on
integrating Bitcoin functionalities, leveraging the NBitcoin library.

## Features

- Seamless integration with the Bitcoin network.
- Leverages NBitcoin for core Bitcoin operations.
- Provides foundational Bitcoin-related services for other NLightning components.

## Installation

Install the package from NuGet:

```bash
dotnet add package NLightning.Infrastructure.Bitcoin
```

## Configuration

This package is designed to work out-of-the-box with minimal configuration, relying on NBitcoin's capabilities.

## Usage

`NLightning.Infrastructure.Bitcoin` is primarily used by other NLightning components that require interaction with the
Bitcoin blockchain or Bitcoin-specific data structures.

## Dependencies

- NBitcoin and NBitcoin.Secp256k1 for all Bitcoin-related operations.
- Microsoft.Extensions.Logging for logging infrastructure.

## Related Projects

- NLightning.Application
- NLightning.Common
- NLightning.Domain
- NLightning.Bolt11
- NLightning.Infrastructure