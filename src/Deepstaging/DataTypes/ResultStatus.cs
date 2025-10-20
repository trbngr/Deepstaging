using Vogen;

namespace Deepstaging.DataTypes;

[ValueObject<string>]
[Instance(name: "Error", value: "Error")]
[Instance(name: "Success", value: "Success")]
public readonly partial struct ResultStatus
{
    public bool Is(ResultStatus status) => Value == status.Value;
}