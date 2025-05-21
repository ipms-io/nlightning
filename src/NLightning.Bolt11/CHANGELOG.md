# Changelog

All notable changes to this project will be documented in this file.

## v3.0.1

Upgrade `NLightning.Infrastructure` to `v0.0.2` to fix an issue with `staticwebassets` not being copied to the
output directory on publish.

### Changed

- Upgraded `NLightning.Infrastructure` to `v0.0.2`.

## v3.0.0

Moved all needed classes to `NLightning.Bolt11` project.

### Added

- Added all base classes to the `NLightning.Bolt11` project;
- Added dependency to `NLightning.Infrastructure`;

### Breaking Changes

- All namespaces have been changed to conform to new project structure;

## v2.0.0

Removal of `SecureKeyManager` static class and introduction of `ISecureKeyManager` interface.

Introduction of `LightningMoney` class to represent amounts in the invoice.

### Removed

- Removed `SecureKeyManager` class;

### Added

- Added `ISecureKeyManager` interface;
- Added `ToString(Key nodeKey)` method to generate the invoice string using a specific key;
- Added `Encode(Key nodeKey)` method to encode the invoice using a specific key;

### Changed

- Invoice public constructors now may receive a `ISecureKeyManager` implementation;

### Breaking Changes

- `Encode` now rely on the `ISecureKeyManager` being passed on the constructors to encode the invoice;
- `ToString` now rely on the `ISecureKeyManager` being passed on the constructors to generate the invoice string;
- Invoice public constructors now receive a `LightningMoney` object instead of `ulong`;
- Removed `AmountMilliSats` and `AmountSats` in favor of the `Amount` property;

## v1.0.0

Removal of `ConfigManager` static class.

### Removed

- Removed `ConfigManager` class;
- Removed obsolete `NLightning.Bolt11.Invoice` class;

### Changed

- Decode method can receive a `Network` object to check if the invoice is valid for the network;

### Breaking Changes

- Invoice public constructors now need to receive a `Network` object;
- Invoice static constructors now need to receive a `Network` object;

## v0.2.4

Update `BitReader` and `BitWriter` to be fully managed

### Changed

- Dropped `unsafe` modifier from `BitReader` and `BitWriter`
- Refactored `BitReader` and `BitWriter` to use managed objects

## v0.2.3

Fix Invoice.TaggedFields' Collection item update behavior

### Added

- Added `EventHandler` on `TaggedFieldList`, `RoutingInfoCollection`, and `Features` to inform subscribers upon changes in the collection;

### Changed

- `Invoice` subscribes to changes on new `Changed` events from contained Lists;

## v0.2.2

Set `CheckForOverflowUnderflow` to false when package is wasm (Blazor)

### Added

- Added `<CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>` under
  `<PropertyGroup Condition="'$(UsingMicrosoftNETSdkBlazorWebAssembly)' == 'true' Or $(Configuration.Contains('.Wasm'))">`
  on `.csproj` file;

## v0.2.1

Fix Assembly name for blazor package

### Added

- Added `<AssemblyName>NLightning.Bolt11.Blazor</AssemblyName>` under
  `<PropertyGroup Condition="'$(UsingMicrosoftNETSdkBlazorWebAssembly)' == 'true' Or $(Configuration.Contains('.Wasm'))">`
  on `.csproj` file;

## v0.2.0

Add WebAssembly capabilities

### Added

- WebAssembly capabilities to enable running code in the browser;
- New package `NLightning.Bolt11.Blazor` on nuget. _see [readme](README.md#decode-the-invoice) for instructions;_

### Changed

- Move Crypto classes from `Bolts` to `Common` and made it public;

### Deprecated

- Deprecated InvoiceProxy `NLightning.Bolt11.Invoice` in favor of using `NLightning.Bolts.BOLT11.Invoice` directly.
This class will be removed on version `v1.x.x`;

## v0.1.0

Introduction of Invoice Proxy

### Added

- `InvoiceProxy` class to make it easier to import from the `NLightning.Bolt11` package;

## v0.0.1

Initial release