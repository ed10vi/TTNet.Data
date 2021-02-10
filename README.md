# TTNet.Data
A .NET library for The Things Network Data API

## Install
Available at [NuGet](https://www.nuget.org/packages/TTNet.Data):

    dotnet add package TTNet.Data

## Usage
Create an instance:
```csharp
var app = new App("Your app ID");
```

Listen to events:
```csharp
app.Connected += (s, e) => Console.WriteLine("Connected");

app.MessageReceived += (s, e) =>
{
    Console.WriteLine(e.DeviceID);
    foreach (var f in e.Message.PayloadFields.EnumerateObject())
        Console.WriteLine($"\t{f.Name}: {f.Value}");

    // Or you can convert it to your data type
    var myObject = e.Message.PayloadFields.ConvertTo<MyClass>();
};
```

Connect:
```csharp
var c = await app.Connect(cluster, port, withTls, username, apiKey);
```

Example:
```csharp
var c = await app.Connect("eu1.cloud.thethings.network", 8883, true, "username@ttn", "XXXXX.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
```

You can add or remove events wether connected or disconnected.

Publish messages:
```csharp
// Raw payload
await app.Publish(deviceID, new byte[] { 0x10, 0xF1 }, port);
// JSON payload
await app.Publish(deviceID, myObject, port);
```

### Managed

Managed mode is also available. The client is started once and will mantain the connection automatically including reconnecting.

Create a managed instance:
```csharp
var app = new ManagedApp("Your app ID");
```

Use `Start/Stop` instead of `Connect/Disconnect`:
```csharp
await app.Start(cluster, port, withTls, username, apiKey, autoReconnectDelay);
```

Publish messages:
```csharp
// Raw payload
await app.Publish(deviceID, new byte[] { 0x10, 0xF1 }, port);
// JSON payload
await app.Publish(deviceID, myObject, port);
```

### JSON Payload

Only properties are converted from and to JSON, and they can be private.

The property name can be overriden with JsonPropertyName attribute.

If JsonIgnore attribute is set, the property will be ignored.

Example:
```csharp
class MyClass
{
    public bool MyBoolean { get; private set; }

    [JsonPropertyName("my_int")]
    public int MyInt { get; private set; }

    [JsonIgnore]
    public string MyString { get; set; }
}
```

Result JSON:
```json
{"MyBoolean":false,"my_int":0}
```