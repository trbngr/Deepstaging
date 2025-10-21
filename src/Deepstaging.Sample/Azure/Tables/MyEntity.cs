using Deepstaging.Azure.Tables;

namespace Deepstaging.Sample.Azure.Tables;

public record MyProperty;

[AzureTableEntity(partitionKey: nameof(ThreadTs), rowKey: nameof(Ts), tableName: nameof(MyEntity))]
public partial record MyEntity
{
    public required byte[] ByteArrayProperty { get; init; }
    public required bool BooleanProperty { get; init; }
    public DateTime DateTimeProperty { get; init; }
    public DateTimeOffset DateTimeOffsetProperty { get; init; }
    public double DoubleProperty { get; init; }
    public Guid GuidProperty { get; init; }
    public int Int32Property { get; init; }
    public long Int64Property { get; init; }
    
    // Nullable variants
    public bool? NullableBooleanProperty { get; init; }
    public DateTime? NullableDateTimeProperty { get; init; }
    public DateTimeOffset? NullableDateTimeOffsetProperty { get; init; }
    public double? NullableDoubleProperty { get; init; }
    public Guid? NullableGuidProperty { get; init; }
    public int? NullableInt32Property { get; init; }
    public long? NullableInt64Property { get; init; }

    public required string ThreadTs { get; set; }
    public required string Ts { get; set; }

    public required MyProperty Property { get; init; }
}