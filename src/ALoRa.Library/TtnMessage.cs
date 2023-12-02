using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ALoRa.Library;

public record TtnMessage
{

    public TtnMessage(byte[] rawMessage, LoRaMessage? msg, string topic, byte[]? payload)
    {
        RawMessage = rawMessage;
        Topic = topic;
        LoraMessage = msg;
        ReceivedAt = msg?.ReceivedAt;
        Payload = payload;
        DeviceId = msg?.EndDeviceIds?.DeviceId;
    }

    public DateTimeOffset? ReceivedAt { get; set; }

    public string? DeviceId { get; set; }

    public LoRaMessage? LoraMessage { get; set; }

    public string Topic { get; set; }

    public byte[]? Payload { get; set; }

    public byte[] RawMessage { get; set; }

    public static TtnMessage DeserializeMessage(byte[] message, string topic)
    {
        var text = Encoding.UTF8.GetString(message);

        var lora = JsonSerializer.Deserialize<LoRaMessage>(text);

        var payload = lora?.UplinkMessage?.DecodedPayload?.Bytes switch
        {
            not null => lora.UplinkMessage?.DecodedPayload?.Bytes
                .Select(Convert.ToByte)
                .ToArray(),
            _ => null,
        };

        var msg = new TtnMessage(message, lora, topic, payload);

        return msg;
    }
}