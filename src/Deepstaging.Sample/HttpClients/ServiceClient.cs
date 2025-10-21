using System.Collections.Immutable;
using System.ComponentModel;
using Deepstaging.Configuration;
using Deepstaging.HttpClient;

namespace Deepstaging.Sample.HttpClients;

[RegisterConfiguration]
public record ServiceClientConfig
{
    [Description("The base url of the service")]
    public required Uri BaseUrl { get; init; }

    [Secret, Description("The API key for authenticating with the service")]
    public required string ApiKey { get; init; }
}

[HttpClient(name: nameof(ServiceClient), configuration: typeof(ServiceClientConfig))]
public partial class ServiceClient
{
    [HttpPost("mif-get-clients-by-phone")]
    private partial Task<ImmutableList<ClientInfo>> GetClients(string phoneNumber);
    
    [HttpPost("messaging-send-sms")]
    private partial Task SendSms(string phoneNumber, string message);
    
    private static ServiceClientReq ConfigureRequest(ServiceClientReq request) => request switch
    {
        GetClients req => req
            .WithBody(number => new { member_phone = number })
            .WithOnSuccess(clients => clients)
            .WithOnError(e => throw e),
        SendSms req => req
            .WithBody((number, message) => new { to = number, message })
            .WithOnError(e => throw e)
            .WithOnSuccess(() => { }),
        _ => throw new ArgumentOutOfRangeException(nameof(request))
    };
}