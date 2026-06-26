// Test ID structs — the generator runs on this project, so these get their
// implementations emitted at build time.

[StronglyTypedId]
public readonly partial struct DeviceId;

[StronglyTypedId(BackingType.Int)]
public readonly partial struct OrderId;

[StronglyTypedId(BackingType.Long)]
public readonly partial struct EventSequenceId;

[StronglyTypedId(BackingType.String)]
public readonly partial struct TagId;
