# Changelog

All notable changes to this project will be documented in this file.

## v0.0.2

Disabled the `Build Compression` for the `*.Wasm` target. This was causing issues with the `dotnet publish` command,
which was not able to resolve the `*.br` files.

### Added

- Added `<DisableBuildCompression>true</DisableBuildCompression>` to the `*.Wasm` target in the project file.

## v0.0.1

Initial release