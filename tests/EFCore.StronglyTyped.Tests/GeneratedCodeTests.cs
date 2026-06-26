/// <summary>Tests for the generated <see cref="DeviceId"/> (Guid backing).</summary>
public class DeviceIdTests
{
    private static readonly Guid _rawGuid = Guid.NewGuid();
    private static readonly DeviceId _id = new(_rawGuid);

    [Fact]
    public void New_ReturnsNonEmptyId()
        => DeviceId.New().Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void Empty_HasEmptyGuidValue()
        => DeviceId.Empty.Value.Should().Be(Guid.Empty);

    [Fact]
    public void Constructor_StoresValue()
        => _id.Value.Should().Be(_rawGuid);

    [Fact]
    public void Equality_SameValue_IsEqual()
        => new DeviceId(_rawGuid).Should().Be(_id);

    [Fact]
    public void Equality_DifferentValue_IsNotEqual()
        => DeviceId.New().Should().NotBe(_id);

    [Fact]
    public void EqualityOperator_SameValue_IsTrue()
        => (new DeviceId(_rawGuid) == _id).Should().BeTrue();

    [Fact]
    public void InequalityOperator_DifferentValue_IsTrue()
        => (DeviceId.New() != _id).Should().BeTrue();

    [Fact]
    public void ExplicitCastToGuid_ReturnsValue()
        => ((Guid)_id).Should().Be(_rawGuid);

    [Fact]
    public void ExplicitCastFromGuid_ReturnsId()
        => ((DeviceId)_rawGuid).Should().Be(_id);

    [Fact]
    public void ToString_ReturnsGuidString()
        => _id.ToString().Should().Be(_rawGuid.ToString());

    [Fact]
    public void GetHashCode_SameValue_SameHash()
        => new DeviceId(_rawGuid).GetHashCode().Should().Be(_id.GetHashCode());

    [Fact]
    public void CompareTo_SameValue_ReturnsZero()
        => _id.CompareTo(new DeviceId(_rawGuid)).Should().Be(0);

    [Fact]
    public void LessThanOperator_SmallerId_IsTrue()
    {
        var small = new DeviceId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var large = new DeviceId(Guid.Parse("00000000-0000-0000-0000-000000000002"));
        (small < large).Should().BeTrue();
    }

    [Fact]
    public void EfCoreValueConverter_RoundTrips()
    {
        var converter = new DeviceId.EfCoreValueConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider(_id)).Should().Be(_id);
    }

    [Fact]
    public void SystemTextJsonConverter_RoundTrips()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_id);
        var result = System.Text.Json.JsonSerializer.Deserialize<DeviceId>(json);
        result.Should().Be(_id);
    }
}

/// <summary>Tests for the generated <see cref="OrderId"/> (int backing).</summary>
public class OrderIdTests
{
    private static readonly OrderId _id = new(42);

    [Fact]
    public void Constructor_StoresValue()
        => _id.Value.Should().Be(42);

    [Fact]
    public void Equality_SameValue_IsEqual()
        => new OrderId(42).Should().Be(_id);

    [Fact]
    public void EqualityOperator_SameValue_IsTrue()
        => (new OrderId(42) == _id).Should().BeTrue();

    [Fact]
    public void ExplicitCastToInt_ReturnsValue()
        => ((int)_id).Should().Be(42);

    [Fact]
    public void ExplicitCastFromInt_ReturnsId()
        => ((OrderId)42).Should().Be(_id);

    [Fact]
    public void ToString_ReturnsIntString()
        => _id.ToString().Should().Be("42");

    [Fact]
    public void LessThanOperator_SmallerId_IsTrue()
        => (new OrderId(1) < new OrderId(2)).Should().BeTrue();

    [Fact]
    public void EfCoreValueConverter_RoundTrips()
    {
        var converter = new OrderId.EfCoreValueConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider(_id)).Should().Be(_id);
    }

    [Fact]
    public void SystemTextJsonConverter_RoundTrips()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_id);
        var result = System.Text.Json.JsonSerializer.Deserialize<OrderId>(json);
        result.Should().Be(_id);
    }
}

/// <summary>Tests for the generated <see cref="EventSequenceId"/> (long backing).</summary>
public class EventSequenceIdTests
{
    private static readonly EventSequenceId _id = new(9_999_999_999L);

    [Fact]
    public void Constructor_StoresValue()
        => _id.Value.Should().Be(9_999_999_999L);

    [Fact]
    public void EqualityOperator_SameValue_IsTrue()
        => (new EventSequenceId(9_999_999_999L) == _id).Should().BeTrue();

    [Fact]
    public void EfCoreValueConverter_RoundTrips()
    {
        var converter = new EventSequenceId.EfCoreValueConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider(_id)).Should().Be(_id);
    }

    [Fact]
    public void SystemTextJsonConverter_RoundTrips()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_id);
        var result = System.Text.Json.JsonSerializer.Deserialize<EventSequenceId>(json);
        result.Should().Be(_id);
    }
}

/// <summary>Tests for the generated <see cref="TagId"/> (string backing).</summary>
public class TagIdTests
{
    private static readonly TagId _id = new("firmware-v2");

    [Fact]
    public void Constructor_StoresValue()
        => _id.Value.Should().Be("firmware-v2");

    [Fact]
    public void Equality_SameValue_IsEqual()
        => new TagId("firmware-v2").Should().Be(_id);

    [Fact]
    public void EqualityOperator_SameValue_IsTrue()
        => (new TagId("firmware-v2") == _id).Should().BeTrue();

    [Fact]
    public void CompareTo_UsesOrdinalComparison()
        => new TagId("a").CompareTo(new TagId("b")).Should().BeNegative();

    [Fact]
    public void EfCoreValueConverter_RoundTrips()
    {
        var converter = new TagId.EfCoreValueConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider(_id)).Should().Be(_id);
    }

    [Fact]
    public void SystemTextJsonConverter_RoundTrips()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_id);
        var result = System.Text.Json.JsonSerializer.Deserialize<TagId>(json);
        result.Should().Be(_id);
    }
}
