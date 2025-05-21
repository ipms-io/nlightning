# NLightning.Application

NLightning.Application is a core component of the NLightning ecosystem, providing application services and business
logic for Lightning Network implementations in .NET. This library implements the use cases and orchestrates domain
objects following clean architecture principles.

## Features

- Application services for Lightning Network operations
- Command and query handlers for domain operations
- Transaction and payment channel management
- Integration with domain events and validators
- Configuration management via Microsoft.Extensions.Options

## Installation

Install the package from NuGet:

```bash
dotnet add package NLightning.Application
```

## Usage

NLightning.Application provides the application layer services to interact with the Lightning Network domain:

```csharp
// Example usage with application services
// Documentation coming soon
```

## Dependencies

- NLightning.Domain for core domain models and services
- Microsoft.Extensions.Options for configuration management

## Related Projects

- NLightning.Infrastructure
- NLightning.Domain
- NLightning.Common
- NLightning.Bolt11