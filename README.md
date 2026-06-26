# EFCore.StronglyTyped

[![NuGet](https://img.shields.io/nuget/v/EFCore.StronglyTyped.svg)](https://www.nuget.org/packages/EFCore.StronglyTyped)
[![CI](https://github.com/Swevo/EFCore.StronglyTyped/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/EFCore.StronglyTyped/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Compile-time strongly-typed ID generation for .NET using Roslyn source generators.

Add `[StronglyTypedId]` to any `readonly partial struct` and get a fully-featured value type with **EF Core ValueConverter** and **System.Text.Json converter** — all generated at build time. Zero reflection, AOT-safe, no runtime overhead.

```csharp
[StronglyTypedId]
public readonly partial struct DeviceId;

// Generated for you:
//  ✔ Value property (Guid, int, long, or string)
//  ✔ Constructor, New(), Empty
//  ✔ IEquatable<T>, IComparable<T>
//  ✔ ==, !=, <, >, <=, >= operators
//  ✔ Explicit cast operators
//  ✔ DeviceId.EfCoreValueConverter (nested class)
//  ✔ DeviceId.SystemTextJsonConverter (nested class, auto-registered via [JsonConverter])
```

---

## Why strongly-typed IDs?

Primitive obsession — using raw `Guid`, `int`, or `string` for IDs — is a silent source of bugs:

```csharp
// Compiles fine. Swapped arguments cause a hard-to-find runtime bug.
void AssignDevice(Guid customerId, Guid deviceId) { ... }
AssignDevice(device.Id, customer.Id); // ← wrong order, no compiler error
```

Strongly-typed IDs turn these mistakes into compiler errors:

```csharp
void AssignDevice(CustomerId customerId, DeviceId deviceId) { ... }
AssignDevice(device.Id, customer.Id); // ← CS1503: cannot convert DeviceId to CustomerId
```

`EFCore.StronglyTyped` makes the pattern zero-friction — one attribute, everything else generated.

---

## Installation

```
dotnet add package EFCore.StronglyTyped
```

Requires .NET 6+ (or any target that supports `netstandard2.0` packages).  
EF Core is **not** a required dependency of the generator — the `EfCoreValueConverter` nested class is only needed when you use EF Core in your project.

---

## Quick start

### 1. Declare your ID types

```csharp
using EFCore.StronglyTyped;

[StronglyTypedId]                       // default: Guid backing
public readonly partial struct DeviceId;

[StronglyTypedId(BackingType.Int)]
public readonly partial struct OrderId;

[StronglyTypedId(BackingType.Long)]
public readonly partial struct EventSequenceId;

[StronglyTypedId(BackingType.String)]
public readonly partial struct TagId;
```

### 2. Use the generated members

```csharp
// Guid-backed — factory method and empty sentinel
var id     = DeviceId.New();          // new random DeviceId
var empty  = DeviceId.Empty;          // DeviceId wrapping Guid.Empty
var parsed = new DeviceId(someGuid);

// Equality and comparison work out of the box
id == DeviceId.New();   // false
id != empty;            // true
id.CompareTo(empty);    // non-zero

// Explicit casts (safe, no silent widening)
Guid raw = (Guid)id;
DeviceId back = (DeviceId)raw;
```

### 3. Wire up EF Core

Register the value converters on individual properties:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Device>()
        .Property(d => d.Id)
        .HasConversion(new DeviceId.EfCoreValueConverter());
}
```

Or apply all converters at once with a convention:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder builder)
{
    builder.Properties<DeviceId>().HaveConversion<DeviceId.EfCoreValueConverter>();
    builder.Properties<OrderId>().HaveConversion<OrderId.EfCoreValueConverter>();
}
```

### 4. JSON serialization

`[JsonConverter]` is applied automatically to every generated struct, so `System.Text.Json` serializes them transparently:

```csharp
var device = new { Id = DeviceId.New(), Name = "Printer-01" };
var json   = JsonSerializer.Serialize(device);
// {"Id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","Name":"Printer-01"}

var back = JsonSerializer.Deserialize<DeviceWrapper>(json);
// back.Id is a DeviceId, not a raw Guid
```

---

## Backing types

| `BackingType`      | C# type  | `New()` | `Empty` | JSON read          | JSON write           |
|--------------------|----------|---------|---------|--------------------|----------------------|
| `BackingType.Guid` | `Guid`   | ✔       | ✔       | `reader.GetGuid()` | `WriteStringValue()` |
| `BackingType.Int`  | `int`    | ✗       | ✗       | `reader.GetInt32()`| `WriteNumberValue()` |
| `BackingType.Long` | `long`   | ✗       | ✗       | `reader.GetInt64()`| `WriteNumberValue()` |
| `BackingType.String`| `string` | ✗       | ✗       | `reader.GetString()`| `WriteStringValue()`|

---

## Generated API surface

For a `[StronglyTypedId]` struct named `DeviceId` with `Guid` backing, the generator emits:

```csharp
[JsonConverter(typeof(DeviceId.SystemTextJsonConverter))]
public readonly partial struct DeviceId : IEquatable<DeviceId>, IComparable<DeviceId>
{
    public Guid Value { get; }
    public DeviceId(Guid value);

    public static DeviceId New();       // Guid only
    public static DeviceId Empty { get; } // Guid only

    public bool Equals(DeviceId other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();
    public int CompareTo(DeviceId other);

    public static bool operator ==(DeviceId left, DeviceId right);
    public static bool operator !=(DeviceId left, DeviceId right);
    public static bool operator  <(DeviceId left, DeviceId right);
    public static bool operator  >(DeviceId left, DeviceId right);
    public static bool operator <=(DeviceId left, DeviceId right);
    public static bool operator >=(DeviceId left, DeviceId right);

    public static explicit operator Guid(DeviceId id);
    public static explicit operator DeviceId(Guid value);

    public sealed class EfCoreValueConverter
        : ValueConverter<DeviceId, Guid> { ... }

    public sealed class SystemTextJsonConverter
        : JsonConverter<DeviceId> { ... }
}
```

---

## Diagnostics

| ID       | Severity | Description |
|----------|----------|-------------|
| `STID001` | Error   | `[StronglyTypedId]` applied to a struct that is not declared `partial`. |

---

## AOT / NativeAOT

Because all code is generated at build time and the `EfCoreValueConverter` uses compile-time lambda expressions, `EFCore.StronglyTyped` is fully compatible with NativeAOT and Blazor WASM trimming.

---

## Related packages

The `EFCore.StronglyTyped` package sits alongside the other compile-time generators in the Swevo suite:

| Package | Purpose |
|---------|---------|
| [AutoWire](https://github.com/Swevo/AutoWire) | Compile-time DI registration |
| [AutoMap.Generator](https://github.com/Swevo/AutoMap.Generator) | Compile-time object mapping |
| [AutoValidate.Generator](https://github.com/Swevo/AutoValidate.Generator) | Compile-time FluentValidation wiring |

---

## License

MIT © [Justin Bannister](https://github.com/Swevo)
