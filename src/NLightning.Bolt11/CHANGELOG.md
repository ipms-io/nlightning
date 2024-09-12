# Changelog

All notable changes to this project will be documented in this file.

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