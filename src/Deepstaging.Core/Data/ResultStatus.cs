using Vogen;

namespace Deepstaging.Data;

[ValueObject<string>]
[Instance(name: "Error", value: "Error")]
[Instance(name: "Success", value: "Success")]
[Instance(name: "Exception", value: "Exception")]
public readonly partial struct ResultStatus
{
    public bool Is(ResultStatus status) => Value == status.Value;
}