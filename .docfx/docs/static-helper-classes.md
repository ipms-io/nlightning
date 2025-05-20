# Static Helper Classes

This section provides an overview of the static helper classes in the NLightning project.

## Bit Utilities
- **[`EndianBitConverter`](xref:NLightning.Infrastructure.Converters.EndianBitConverter)** - Provides bitwise conversion utilities.

## Constants
- **[`ChainConstants`](xref:NLightning.Domain.Protocol.Constants.ChainConstants)** - Holds predefined chain-related constants.
- **[`CryptoConstants`](xref:NLightning.Domain.Crypto.Constants.CryptoConstants)** - Defines cryptographic constants.
- **[`DaemonConstants`](xref:NLightning.Application.NLTG.Constants.DaemonConstants)** - Contains constants related to the daemon.
- **[`InvoiceConstants`](xref:NLightning.Bolt11.Constants.InvoiceConstants)** - Contains constants used in Bolt11 invoices.
- **[`TaggedFieldConstants`](xref:NLightning.Bolt11.Constants.TaggedFieldConstants)** - Constants for tagged fields in Bolt11.

## Validators
- **[`TxAddInputValidator`](xref:NLightning.Infrastructure.Protocol.Validators.TxAddInputValidator)** - Validates added inputs in a transaction.
- **[`TxAddOutputValidator`](xref:NLightning.Infrastructure.Protocol.Validators.TxAddOutputValidator)** - Validates added outputs in a transaction.
- **[`TxCompleteValidator`](xref:NLightning.Infrastructure.Protocol.Validators.TxCompleteValidator)** - Ensures transaction completeness.
- **[`TxRemoveInputValidator`](xref:NLightning.Infrastructure.Protocol.Validators.TxRemoveInputValidator)** - Validates removed inputs.
- **[`TxRemoveOutputValidator`](xref:NLightning.Infrastructure.Protocol.Validators.TxRemoveOutputValidator)** - Validates removed outputs.

## Factories
- **[`ChannelIdFactory`](xref:NLightning.Common.Factories.ChannelIdFactory)** - Generates channel IDs.

## Other Utilities
- **[`ExceptionUtils`](xref:NLightning.Common.Utils.ExceptionUtils)** - Helper methods for handling exceptions.
- **[`BlazorCryptoProvider`](xref:NLightning.Common.Crypto.Providers.JS.BlazorCryptoProvider)** - Crypto provider for Blazor.

## Test Helpers
*(Only used in tests)*
- **[`StaticAssetsHelper`](xref:NLightning.Blazor.Tests.Helpers.StaticAssetsHelper)** - Manages static assets in tests.
- **[`PortPool`](xref:NLightning.Bolts.Tests.BOLT1.Fixtures.PortPool)** - Handles port allocation in test environments.
- **[`TestUtils`](xref:NLightning.Common.Tests.Utils.TestUtils)** - General test utilities.
- **[`TestHexConverter`](xref:NLightning.Bolts.Tests.Utils.TestHexConverter)** - Converts hex data for testing.
- **[`AeadChacha20Poly1305IetfVector`](xref:NLightning.Common.Tests.Vectors.AeadChacha20Poly1305IetfVector)** - Vector tests for AEAD encryption.
