namespace EFCore.StronglyTyped;

/// <summary>The primitive type that backs a strongly-typed ID.</summary>
internal enum BackingTypeKind
{
    /// <summary>Backed by <see cref="System.Guid"/>.</summary>
    Guid = 0,

    /// <summary>Backed by <see cref="int"/>.</summary>
    Int = 1,

    /// <summary>Backed by <see cref="long"/>.</summary>
    Long = 2,

    /// <summary>Backed by <see cref="string"/>.</summary>
    String = 3,
}

/// <summary>Immutable model extracted from a <c>[StronglyTypedId]</c> struct declaration.</summary>
internal sealed record TypeModel(
    string TypeName,
    string? Namespace,
    BackingTypeKind BackingType);
