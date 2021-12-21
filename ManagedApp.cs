using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading.Tasks;

namespace TTNet.Data;

/// <summary>
/// A The Things Network Application Data connection.
/// </summary>
public class ManagedApp : AppBase
{
    private new IManagedMqttClient ManagedMqttClient => base.ManagedMqttClient!;

    /// <summary>
    /// Value indicating whether this <see cref="T:TTNet.Data.ManagedApp"/> is connected.
    /// </summary>
    public override bool IsConnected => ManagedMqttClient.IsConnected == true;

    /// <summary>
    /// Count of pending messages to send.
    /// </summary>
    public int PendingMessagesCount
    {
        get => ManagedMqttClient.PendingApplicationMessagesCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TTNet.Data.ManagedApp"/> class.
    /// </summary>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public ManagedApp(string appId, string? tenantId = "ttn") : base(new MqttFactory().CreateManagedMqttClient(), appId, tenantId)
    {
        ManagedMqttClient.ConnectedHandler = this;
        ManagedMqttClient.DisconnectedHandler = this;
        ManagedMqttClient.ApplicationMessageReceivedHandler = this;

        ManagedMqttClient.ApplicationMessageProcessedHandler = this;
        ManagedMqttClient.ApplicationMessageSkippedHandler = this;
        ManagedMqttClient.ConnectingFailedHandler = this;
        ManagedMqttClient.SynchronizingSubscriptionsFailedHandler = this;
    }

    /// <summary>
    /// Start connection to The Things Network server.
    /// </summary>
    /// <param name="server">Server domain name.</param>
    /// <param name="port">Connection port.</param>
    /// <param name="withTls">Use TLS.</param>
    /// <param name="username">Username.</param>
    /// <param name="apiKey">API access key.</param>
    /// <param name="autoReconnectDelay">Time to wait after a disconnection to reconnect.</param>
    public Task StartAsync(string server, int port, bool withTls, string username, string apiKey, TimeSpan autoReconnectDelay) =>
        ManagedMqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(autoReconnectDelay)
            .WithClientOptions(GetMqttClientOptions(server, port, withTls, username, apiKey))
            .Build());

    /// <summary>
    /// Stop connection.
    /// </summary>
    public Task StopAsync() => ManagedMqttClient.StopAsync();

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
    public override void Dispose() => ManagedMqttClient.Dispose();
}
