# TTNet.Data
A .NET library for The Things Network Data API

## Install
Available at [NuGet](https://www.nuget.org/packages/TTNet.Data):

    dotnet add package TTNet.Data

## Usage
Create an instance:

    var app = new App("Your app ID");

Listen to events:

    app.Connected += (s, e) => Console.WriteLine("Connected");

    app.MessageReceived += (s, e) =>
    {
        Console.WriteLine(e.DeviceID);
        foreach (var f in e.Message.PayloadFields.EnumerateObject())
            Console.WriteLine($"\t{f.Name}: {f.Value}");

        // Or you can convert it to your data type
        var myObject = e.Message.PayloadFields.ConvertTo<MyClass>();
    };

Connect:

    var c = await app.Connect(accessKey, region, CancellationToken.None);

Example:

    await app.Connect("ttn-account-v2.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "eu", CancellationToken.None);

You can add or remove events wether connected or disconnected.

Publish messages:

    // Raw payload
    await app.Publish(deviceID, new byte[] { 0x10, 0xF1 }, port, CancellationToken.None);
    // JSON payload
    await app.Publish(deviceID, myObject, port, CancellationToken.None);

### JSON Payload

Only properties are converted from and to JSON, and they can be private.

The property name can be overriden with JsonPropertyName attribute.

If JsonIgnore attribute is set, the property will be ignored.

Example:

    class MyClass
    {
        public bool MyBoolean { get; private set; }

        [JsonPropertyName("my_int")]
        public int MyInt { get; private set; }

        [JsonIgnore]
        public string MyString { get; set; }
    }

Result JSON:

    {"MyBoolean":false,"my_int":0}