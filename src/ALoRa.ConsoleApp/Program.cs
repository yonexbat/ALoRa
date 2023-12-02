using System;
using System.Reflection;
using System.Text;
using System.Threading;
using ALoRa.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ALoRa.ConsoleApp;

internal static class Program
{
    static private void Main(string[] args)
    {
        Console.WriteLine("\nALoRa ConsoleApp - A The Things Network C# Library\n");

        var config = GetConfiguration(args);

        var appId = config.GetValue<string>("appId") ?? throw new ArgumentNullException("appId");
        var accessKey = config.GetValue<string>("accessKey") ?? throw new ArgumentNullException("accessKey");
        var region = config.GetValue<string>("region") ?? throw new ArgumentNullException("region");
        var deviceId = config.GetValue<string>("deviceId") ?? throw new ArgumentNullException("deviceId");

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<TtnApplication> logger = factory.CreateLogger<TtnApplication>();

        using var app = new TtnApplication(appId, accessKey, region, logger);
        app.MessageReceived += App_MessageReceived;

        Console.WriteLine("Press return for next!");
        Console.ReadLine();
        app.Publish(deviceId, Array.Empty<byte>());
        
        Console.ReadLine();

        Console.WriteLine("\nAloha, Goodbye, Vaarwel!");

        Thread.Sleep(1000);
    }

    static private IConfiguration GetConfiguration(string[] args)
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddCommandLine(args)
            .Build();
    }

    static private void App_MessageReceived(TtnMessage obj)
    {
        var data = obj.Payload switch
        {
            not null => BitConverter.ToString(obj.Payload),
            _ => string.Empty,
        };

        var payLoadAsString = obj.Payload switch
        {
            not null => Encoding.UTF8.GetString(obj.Payload),
            _ => string.Empty,
        };

        Console.WriteLine(
            $"Message ReceivedAt: {obj.ReceivedAt}, Device: {obj.DeviceId}, Topic: {obj.Topic}, Payload: {data}, StringPayload: {payLoadAsString}");
    }
}