using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace EFCore.StronglyTyped;

/// <summary>Roslyn incremental source generator that emits strongly-typed ID implementations
/// for any <c>readonly partial struct</c> decorated with <c>[StronglyTypedId]</c>.</summary>
[Generator]
public sealed class StronglyTypedIdGenerator : IIncrementalGenerator
{
    private const string AttributeFqn = "EFCore.StronglyTyped.StronglyTypedIdAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Always emit the attribute + enum into every consuming compilation.
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource(
                "StronglyTypedIdAttribute.g.cs",
                SourceText.From(Emitter.AttributeSource, Encoding.UTF8)));

        // Find all structs decorated with [StronglyTypedId].
        var typeModels = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFqn,
                predicate: static (node, _) => node is StructDeclarationSyntax,
                transform: static (ctx, _) => Parse(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        context.RegisterSourceOutput(typeModels, static (ctx, model) =>
        {
            var source = Emitter.Emit(model);
            ctx.AddSource($"{model.TypeName}.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        // Separate pipeline for diagnostics on non-partial structs.
        var diagnostics = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFqn,
                predicate: static (node, _) => node is StructDeclarationSyntax,
                transform: static (ctx, _) => GetDiagnostic(ctx))
            .Where(static d => d is not null)
            .Select(static (d, _) => d!);

        context.RegisterSourceOutput(diagnostics, static (ctx, diag) => ctx.ReportDiagnostic(diag));
    }

    // ── Parsing ───────────────────────────────────────────────────────────────

    private static TypeModel? Parse(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        // Must be partial — a diagnostic is emitted by GetDiagnostic if it is not.
        if (!IsPartial(ctx.TargetNode))
            return null;

        var backingType = BackingTypeKind.Guid;

        var attribute = ctx.Attributes[0];
        if (attribute.ConstructorArguments.Length > 0
            && attribute.ConstructorArguments[0].Value is int raw)
        {
            backingType = (BackingTypeKind)raw;
        }

        var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        return new TypeModel(typeSymbol.Name, namespaceName, backingType);
    }

    private static Diagnostic? GetDiagnostic(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetNode is not StructDeclarationSyntax structDecl)
            return null;

        if (IsPartial(ctx.TargetNode))
            return null;

        return Diagnostic.Create(
            Diagnostics.MustBePartial,
            structDecl.Identifier.GetLocation(),
            structDecl.Identifier.Text);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsPartial(SyntaxNode node)
    {
        if (node is not StructDeclarationSyntax structDecl)
            return false;

        foreach (var modifier in structDecl.Modifiers)
        {
            if (modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
                return true;
        }

        return false;
    }
}
