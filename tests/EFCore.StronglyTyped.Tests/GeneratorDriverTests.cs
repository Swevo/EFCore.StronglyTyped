using EFCore.StronglyTyped;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>Validates generator output via <see cref="CSharpGeneratorDriver"/> without
/// compiling the generated source against EF Core.</summary>
public class GeneratorDriverTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (Compilation input, GeneratorDriverRunResult result) RunGenerator(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StronglyTypedIdGenerator).Assembly.Location),
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new StronglyTypedIdGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .RunGenerators(compilation);

        return (compilation, driver.GetRunResult());
    }

    private static string GetGeneratedSource(GeneratorDriverRunResult result)
    {
        // Tree 0 is the embedded attribute, tree 1+ are the type implementations.
        result.GeneratedTrees.Should().HaveCountGreaterThanOrEqualTo(2);
        return result.GeneratedTrees[1].ToString();
    }

    // ── Attribute emission ────────────────────────────────────────────────────

    [Fact]
    public void AttributeIsAlwaysEmitted()
    {
        var (_, result) = RunGenerator("// no types");

        result.GeneratedTrees.Should().HaveCount(1);
        result.GeneratedTrees[0].ToString().Should().Contain("class StronglyTypedIdAttribute");
        result.GeneratedTrees[0].ToString().Should().Contain("enum BackingType");
    }

    // ── Guid backing (default) ────────────────────────────────────────────────

    [Fact]
    public void GuidBacking_GeneratesValueProperty()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct DeviceId;
            """);

        GetGeneratedSource(result).Should().Contain("public System.Guid Value");
    }

    [Fact]
    public void GuidBacking_GeneratesNewAndEmptyFactories()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct DeviceId;
            """);

        var src = GetGeneratedSource(result);
        src.Should().Contain("public static DeviceId New()");
        src.Should().Contain("public static DeviceId Empty");
    }

    [Fact]
    public void GuidBacking_GeneratesEfCoreValueConverter()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct DeviceId;
            """);

        GetGeneratedSource(result).Should().Contain("class EfCoreValueConverter");
    }

    [Fact]
    public void GuidBacking_GeneratesSystemTextJsonConverter()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct DeviceId;
            """);

        var src = GetGeneratedSource(result);
        src.Should().Contain("class SystemTextJsonConverter");
        src.Should().Contain("reader.GetGuid()");
        src.Should().Contain("WriteStringValue");
    }

    [Fact]
    public void GuidBacking_GeneratesJsonConverterAttribute()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct DeviceId;
            """);

        GetGeneratedSource(result).Should().Contain("[System.Text.Json.Serialization.JsonConverter(typeof(DeviceId.SystemTextJsonConverter))]");
    }

    // ── Int backing ───────────────────────────────────────────────────────────

    [Fact]
    public void IntBacking_GeneratesIntValueProperty()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.Int)]
            public readonly partial struct OrderId;
            """);

        GetGeneratedSource(result).Should().Contain("public int Value");
    }

    [Fact]
    public void IntBacking_DoesNotGenerateNewOrEmpty()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.Int)]
            public readonly partial struct OrderId;
            """);

        var src = GetGeneratedSource(result);
        src.Should().NotContain("public static OrderId New()");
        src.Should().NotContain("public static OrderId Empty");
    }

    [Fact]
    public void IntBacking_GeneratesNumericJsonWrite()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.Int)]
            public readonly partial struct OrderId;
            """);

        GetGeneratedSource(result).Should().Contain("WriteNumberValue");
    }

    // ── Long backing ──────────────────────────────────────────────────────────

    [Fact]
    public void LongBacking_GeneratesLongValueProperty()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.Long)]
            public readonly partial struct EventSequenceId;
            """);

        GetGeneratedSource(result).Should().Contain("public long Value");
    }

    [Fact]
    public void LongBacking_GeneratesGetInt64JsonRead()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.Long)]
            public readonly partial struct EventSequenceId;
            """);

        GetGeneratedSource(result).Should().Contain("reader.GetInt64()");
    }

    // ── String backing ────────────────────────────────────────────────────────

    [Fact]
    public void StringBacking_GeneratesStringValueProperty()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.String)]
            public readonly partial struct TagId;
            """);

        GetGeneratedSource(result).Should().Contain("public string Value");
    }

    [Fact]
    public void StringBacking_GeneratesOrdinalCompareTo()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId(BackingType.String)]
            public readonly partial struct TagId;
            """);

        GetGeneratedSource(result).Should().Contain("System.StringComparer.Ordinal.Compare");
    }

    // ── Namespace ─────────────────────────────────────────────────────────────

    [Fact]
    public void NamespacedType_WrapsOutputInNamespace()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            namespace MyApp.Domain;
            [StronglyTypedId]
            public readonly partial struct FirmwareId;
            """);

        GetGeneratedSource(result).Should().Contain("namespace MyApp.Domain");
    }

    [Fact]
    public void GlobalNamespaceType_OmitsNamespaceBlock()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly partial struct RootId;
            """);

        GetGeneratedSource(result).Should().NotContain("namespace ");
    }

    // ── Diagnostics ───────────────────────────────────────────────────────────

    [Fact]
    public void NonPartialStruct_ReportsStid001()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly struct NonPartialId;
            """);

        result.Diagnostics.Should().ContainSingle(d => d.Id == "STID001");
    }

    [Fact]
    public void NonPartialStruct_DoesNotGenerateImplementation()
    {
        var (_, result) = RunGenerator("""
            using EFCore.StronglyTyped;
            [StronglyTypedId]
            public readonly struct NonPartialId;
            """);

        // Only the attribute tree should be emitted; no implementation.
        result.GeneratedTrees.Should().HaveCount(1);
    }
}
