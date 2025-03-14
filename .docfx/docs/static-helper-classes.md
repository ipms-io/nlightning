# Static Helper Classes

This section provides an overview of the static helper classes in the NLightning project.

## Bit Utilities
- **[`EndianBitConverter`](xref:NLightning.Common.BitUtils.EndianBitConverter)** - Provides bitwise conversion utilities.
- **[`BitArrayExtension`](xref:NLightning.Common.Extensions.BitArrayExtension)** - Adds extension methods for `BitArray`.

## Constants
- **[`ChainConstants`](xref:NLightning.Common.Constants.ChainConstants)** - Holds predefined chain-related constants.
- **[`CryptoConstants`](xref:NLightning.Common.Constants.CryptoConstants)** - Defines cryptographic constants.
- **[`InvoiceConstants`](xref:NLightning.Bolts.BOLT11.Constants.InvoiceConstants)** - Contains constants used in Bolt11 invoices.
- **[`TaggedFieldConstants`](xref:NLightning.Bolts.BOLT11.Constants.TaggedFieldConstants)** - Constants for tagged fields in Bolt11.

## Validators
- **[`TxAddInputValidator`](xref:NLightning.Bolts.BOLT2.Validators.TxAddInputValidator)** - Validates added inputs in a transaction.
- **[`TxAddOutputValidator`](xref:NLightning.Bolts.BOLT2.Validators.TxAddOutputValidator)** - Validates added outputs in a transaction.
- **[`TxCompleteValidator`](xref:NLightning.Bolts.BOLT2.Validators.TxCompleteValidator)** - Ensures transaction completeness.
- **[`TxRemoveInputValidator`](xref:NLightning.Bolts.BOLT2.Validators.TxRemoveInputValidator)** - Validates removed inputs.
- **[`TxRemoveOutputValidator`](xref:NLightning.Bolts.BOLT2.Validators.TxRemoveOutputValidator)** - Validates removed outputs.

## Factories
- **[`MessageFactory`](xref:NLightning.Bolts.Factories.MessageFactory)** - Factory for creating different message types.
- **[`ChannelIdFactory`](xref:NLightning.Common.Factories.ChannelIdFactory)** - Generates channel IDs.

## Other Utilities
- **[`SecureKeyManager`](xref:NLightning.Common.Managers.SecureKeyManager)** - Manages secure keys.
- **[`ExceptionUtils`](xref:NLightning.Common.Utils.ExceptionUtils)** - Helper methods for handling exceptions.
- **[`BlazorCryptoProvider`](xref:NLightning.Common.Crypto.Providers.JS.BlazorCryptoProvider)** - Crypto provider for Blazor.

## Test Helpers
*(Only used in tests)*
- **[`StaticAssetsHelper`](xref:NLightning.Blazor.Tests.Helpers.StaticAssetsHelper)** - Manages static assets in tests.
- **[`PortPool`](xref:NLightning.Bolts.Tests.BOLT1.Fixtures.PortPool)** - Handles port allocation in test environments.
- **[`TestUtils`](xref:NLightning.Common.Tests.Utils.TestUtils)** - General test utilities.
- **[`TestHexConverter`](xref:NLightning.Bolts.Tests.Utils.TestHexConverter)** - Converts hex data for testing.
- **[`AeadChacha20Poly1305IetfVector`](xref:NLightning.Common.Tests.Vectors.AeadChacha20Poly1305IetfVector)** - Vector tests for AEAD encryption.
