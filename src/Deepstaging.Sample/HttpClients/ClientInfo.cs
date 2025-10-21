using System.Text.Json.Serialization;

namespace Deepstaging.Sample.HttpClients;


public class ClientInfo
{
    [JsonPropertyName("clientID")]
    public int ClientId { get; set; }

    [JsonPropertyName("cin")]
    public string Cin { get; set; } = string.Empty;
}