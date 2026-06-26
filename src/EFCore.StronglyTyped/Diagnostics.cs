using Microsoft.CodeAnalysis;

namespace EFCore.StronglyTyped;

internal static class Diagnostics
{
    /// <summary>Reported when [StronglyTypedId] is placed on a struct that is not declared partial.</summary>
    internal static readonly DiagnosticDescriptor MustBePartial = new(
        id: "STID001",
        title: "StronglyTypedId struct must be partial",
        messageFormat: "'{0}' must be declared as a partial struct to use [StronglyTypedId]",
        category: "EFCore.StronglyTyped",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The [StronglyTypedId] source generator requires the target struct to be declared partial so it can contribute the generated members.");
}
