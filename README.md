# TTNet.Data

A .NET library for The Things Network MQTT Data API

## Install

Available at [NuGet](https://www.nuget.org/packages/TTNet.Data):

```sh
dotnet add package TTNet.Data
```

## Usage

Create an instance:

```csharp
var app = new App("Your app ID");
```

Listen to events:

```csharp
app.Connected += (s, e) => Console.WriteLine("Connected");

app.Up += (s, e) =>
{
    Console.WriteLine(e.DeviceID);
    foreach (JsonProperty f in e.Message.UplinkMessage.DecodedPayload.EnumerateObject())
        Console.WriteLine($"\t{f.Name}: {f.Value}");

    // Or you can convert it to your data type
    var myObject = e.Message.UplinkMessage.DecodedPayload.Deserialize<MyClass>();
};
```

Listen to specific devices:

```csharp
app["deviceId"].Up += (s, e) =>
{
    foreach (JsonProperty f in e.Message.UplinkMessage.DecodedPayload.EnumerateObject())
        Console.WriteLine($"\t{f.Name}: {f.Value}");
};
```

Connect:

```csharp
var c = await app.ConnectAsync(cluster, port, withTls, username, apiKey);
```

See [how to create an API key](https://www.thethingsindustries.com/docs/integrations/mqtt/#creating-an-api-key)

Example:

```csharp
var c = await app.ConnectAsync("eu1.cloud.thethings.network", 8883, true, "username@ttn", "XXXXX.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
```

You can add or remove events wether connected or disconnected.

Publish messages:

```csharp
// Raw payload
await app[deviceID].PublishAsync(new Downlink {
    FPort = port,
    FrmPayload = new byte[] { 0x10, 0xF1 }
}, default(CancellationToken));
// JSON payload
await app[deviceID].PublishAsync(new Downlink {
    FPort = port,
    DecodedPayload = JsonSerializer.SerializeToElement(myObject)
}, default(CancellationToken));
```

### Managed

Managed mode is also available. The client is started once and will mantain the connection automatically including reconnecting.

Create a managed instance:

```csharp
var app = new ManagedApp("Your app ID");
```

Use `StartAsync/StopAsync` instead of `ConnectAsync/DisconnectAsync`:

```csharp
await app.StartAsync(cluster, port, withTls, username, apiKey, autoReconnectDelay);
```

Publish messages:

```csharp
// Raw payload
await app[deviceID].PublishAsync(new Downlink {
    FPort = port,
    FrmPayload = new byte[] { 0x10, 0xF1 }
});
// JSON payload
await app[deviceID].PublishAsync(new Downlink {
    FPort = port,
    DecodedPayload = JsonSerializer.SerializeToElement(myObject)
});
```
