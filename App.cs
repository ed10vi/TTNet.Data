using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System.Threading;
using System.Threading.Tasks;

namespace TTNet.Data;

/// <summary>
/// A The Things Network Application Data connection.
/// </summary>
public class App : AppBase
{
    private new IMqttClient _mqttClient => base._mqttClient!;

    /// <summary>
    /// Value indicating whether this <see cref="TTNet.Data.App"/> is connected.
    /// </summary>
    public override bool IsConnected => _mqttClient.IsConnected == true;

    /// <summary>
    /// Initializes a new instance of the <see cref="TTNet.Data.App"/> class.
    /// </summary>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public App(string appId, string? tenantId = "ttn") : base(new MqttFactory().CreateMqttClient(), appId, tenantId)
    {
        _mqttClient.ConnectedHandler = this;
        _mqttClient.DisconnectedHandler = this;
        _mqttClient.ApplicationMessageReceivedHandler = this;
    }

    /// <summary>
    /// Connect to The Things Network server.
    /// </summary>
    /// <returns>The <see cref="MQTTnet.Client.Connecting.MqttClientConnectResultCode"/>.</returns>
    /// <param name="server">Server domain name.</param>
    /// <param name="port">Connection port.</param>
    /// <param name="withTls">Use TLS.</param>
    /// <param name="username">Username.</param>
    /// <param name="apiKey">API access key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<MqttClientConnectResultCode> ConnectAsync(string server, int port, bool withTls, string username, string apiKey, CancellationToken cancellationToken = default)
    {
        var result = await _mqttClient.ConnectAsync(GetMqttClientOptions(server, port, withTls, username, apiKey), cancellationToken);
        return result.ResultCode;
    }

    /// <summary>
    /// Disconnect from server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);

    private IMqttClientOptions GetMqttClientOptions(string server, int port, bool withTls, string username, string apiKey)
    {
        var o = new MqttClientOptionsBuilder()
            .WithClientId(ClientID)
            .WithTcpServer(server, port)
            .WithCredentials(username, apiKey)
            .WithCleanSession();
        return withTls ? o.WithTls(p => p.SslProtocol = System.Security.Authentication.SslProtocols.None).Build() : o.Build();
    }

    /// <summary>
    /// Dispose all resources used by this object
    /// </summary>
    public override void Dispose() => _mqttClient.Dispose();
}
