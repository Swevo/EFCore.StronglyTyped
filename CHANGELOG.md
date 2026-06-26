# Changelog

All notable changes to `EFCore.StronglyTyped` will be documented in this file.

## [1.0.0] - 2025-06-26

### Added
- `[StronglyTypedId]` attribute with `BackingType` enum (`Guid`, `Int`, `Long`, `String`).
- Generates `IEquatable<T>`, `IComparable<T>`, `==`, `!=`, `<`, `>`, `<=`, `>=` operators.
- Generates explicit cast operators to and from the backing primitive.
- Generates `DeviceId.EfCoreValueConverter` nested class for EF Core integration.
- Generates `DeviceId.SystemTextJsonConverter` nested class; auto-wired via `[JsonConverter]`.
- `New()` factory and `Empty` sentinel for `Guid`-backed types.
- `STID001` diagnostic error when the target struct is not declared `partial`.
