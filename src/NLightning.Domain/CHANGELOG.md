# Changelog

All notable changes to this project will be documented in this file.

## v1.0.0

This version's focus has been on enhancing Bitcoin integration, refining channel management, standardizing protocol
message handling, and improving core domain primitives. These changes aim to solidify the domain logic and provide a
richer, more robust foundation for building Lightning Network features.

### Added

- Added new namespaces, interfaces, and value objects;
- Introduced internal value objects and models to represent Bitcoin-specific concepts (e.g., `TxId`, `BitcoinScript`,
  `BitcoinLockTime`, `CompactPubKey`, `Secret`) to enhance abstraction and reduce external library coupling;
- Introduced comprehensive models for channel components like `ChannelModel`, `ChannelKeySetModel`, and `Htlc`;
- Added detailed value objects for channel configuration (`ChannelConfig`), basepoints (`ChannelBasepoints`), and
  signing information (`ChannelSigningInfo`);
- Introduced new factories and interfaces for message creation and serialization;
- Added various factories (e.g., `ChannelFactory`, `CommitmentTransactionModelFactory`)  and service interfaces (e.g.,
  `ILightningSigner`, `ICommitmentKeyDerivationService`, `ISecretStorageService`) to support the new domain models and
  promote better separation of concerns;
- Moved `BitReader` and `BitWriter` utilities from `NLightning.Common`.

### Modified

- Significant updates to arithmetic operations on `LightningMoney`;
- Updated feature handling logic on `FeatureSet`;
- Adjustments in `NodeOptions.cs` and `FeatureOptions.cs`;
- Restructured protocol messages and payloads (e.g., `OpenChannel1Message`, `AcceptChannel1Payload`) with dedicated
  properties and TLV field support;
- Update existing message and payload classes;
- Moved `Witness` to `NLightning.Domain/Bitcoin/ValueObjects/`;
- Moved `ChannelFlags` to `NLightning.Domain/Channels/ValueObjects/`;
- Moved `ShortChannelId` to `NLightning.Domain/Channels/ValueObjects/`;
- Moved `BigSize` to `NLightning.Domain/Protocol/ValueObjects/`;
- Moved `ChainHash` to `NLightning.Domain/Protocol/ValueObjects/`;
- Moved service interfaces from `Services` to `Interfaces`;
- Serialization interfaces moved to `NLightning.Domain.Serialization.Interfaces`;
- Utility interfaces moved to `NLightning.Domain.Utils.Interfaces`.

### Deleted

- Removed direct dependencies on `NBitcoin` and `NBitcoin.Secp256k1`.
- Removed interfaces `ICommitmentTransactionFactory`, `IFundingTransactionFactory`,  `IHtlcTransactionFactory`,
  `IPingPongServiceFactory`, `IHtlc`.

### Breaking Changes

- `Channel` has moved to `ChannelModel`;
- `HashConstants` has been merged into `Crypto.Constants.CryptoConstants`;
- `ChannelState` has been moved to `NLightning.Domain.Channels.Enums`;
- `ChannelException` has been replaced by more specific exceptions like `ChannelErrorException` and
  `ChannelWarningException`;
- `IMessageFactory` has been moved to `NLightning.Domain.Protocol.Interfaces.IMessageFactory`;
- `TransactionConstants` and `WeightConstants` moved to `NLightning.Domain.Bitcoin.Transactions.Constants`;
- Removed interfaces as per the removed section;
- `ISecureKeyManager` has been moved to `NLightning.Domain.Protocol.Interfaces`;
- Service interfaces in `Services` sub-namespaces (e.g., `IDustService.cs`, `IKeyDerivationService.cs`,
  `ISecretStorageService`)moved to corresponding `Interfaces` namespaces;
- Serialization factory interfaces have been moved to `NLightning.Domain.Serialization.Interfaces`;
- `ChannelId` has been moved to `NLightning.Domain.Channels.ValueObjects/ChannelId.`;
- `IValueObject` has been moved to `NLightning.Domain.Interfaces/IValueObject`;
- `Network` has been moved to `BitcoinNetwork`;
- Many types have been moved to more specific sub-namespaces (e.g., within `Bitcoin`, `Channels`, `Protocol`,
  `Serialization`, `Utils`). This is a breaking change requiring updates to `using` statements.
- The extensive refactoring, addition of new types, and modifications to existing files imply potential changes to
  method signatures, constructors, and property accessors in various parts of the domain.
- The direct package reference to `NBitcoin` has been removed from the project file. Consumers relying on transitive
  NBitcoin types exposed by `NLightning.Domain`'s previous public API may need to add a direct reference to NBitcoin
  or adapt to the new abstractions provided within `NLightning.Domain.Bitcoin.*`.

## v0.0.1

Initial release