# Versioning Policy

This document provides a detailed overview of the versioning strategy adopted for the various packages within the
NLightning repository. Our versioning aims to be transparent and predictable to assist users and contributors in
understanding expected changes.

## Overview

We use [Semantic Versioning 2.0.0 (SemVer)](https://semver.org/) for all packages in the repository. The versioning
format is:
    
```
MAJOR.MINOR.PATCH
```

- MAJOR versions indicate incompatible API changes,
- MINOR versions add functionality in a backward-compatible manner,
- PATCH versions make backward-compatible bug fixes.

## Packages

The repository contains multiple packages, managed as follows:

### Global Version (Common and Bolts Packages)

- Scope: Applies to all shared functionality and base components (NLightning.Common, NLightning.Bolts, etc.).
- Versioning: These packages share a unified version to maintain consistency across core components. Any change
(bug fix, feature, or breaking change) in any of these packages may result in a version increment applicable to all.

### BOLT11 Package

- Scope: Specific to Bitcoin Lightning Invoice processing and operations.
- Versioning: This package follows its own versioning timeline to address the specific evolution of BOLT11 features and
improvements independent of the core components.

## Versioning Triggers

### Global Version

1. Major Update: Changes that cause backward-incompatible modifications or removal of existing functionalities. 
2. Minor Update: Introduction of new features that are backward-compatible. 
3. Patch Update: Backward-compatible bug fixes, performance enhancements, and minor improvements.

### BOLT11 Package

1. Major Update: Significant changes that redefine or remove existing APIs specific to BOLT11. 
2. Minor Update: Backward-compatible enhancements or new features specific to BOLT11 processing. 
3. Patch Update: Minor bug fixes and improvements within the BOLT11 context.

## Tagging and Release

Each release is tagged in the repository with the version number. Tags are created as follows:

- Global tags are prefixed with global-v, e.g., global-v1.0.0.
- BOLT11 tags are prefixed with bolt11-v, e.g., bolt11-v1.0.0.

## Contributing

Contributors are encouraged to follow this versioning policy for any changes made. Proposed changes should include
details in pull requests on whether they constitute major, minor, or patch changes, providing necessary descriptions
and justifications.