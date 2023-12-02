using System.Text;
using ALoRa.Library;
using FluentAssertions;

namespace ALoRa.UnitTests;

public class TtnMessageFixture
{
    [Fact]
    public void DeserialiseMessage_ShouldDeserializeOk()
    {
        // Prepare
        var testMessage = GetMessage();
        var bytes = Encoding.UTF8.GetBytes(testMessage);

        // Act
        var loRaMessage = TtnMessage.DeserializeMessage(bytes, "testTopic");

        // Assert
        loRaMessage.Should().NotBeNull();
        loRaMessage.Payload.Should().NotBeNull();
        loRaMessage.LoraMessage.Should().NotBeNull();
        loRaMessage.RawMessage.Should().NotBeNull();
        loRaMessage.DeviceId.Should().Be("eui-a8610a303920760b");
        loRaMessage.ReceivedAt.Should().BeCloseTo(DateTimeOffset.Parse("2023-12-02T10:13:08.998975993Z"), new TimeSpan(0, 0, 0, 1));
    }

    private string GetMessage()
    {
        var assembly = typeof(TtnMessageFixture).Assembly;
        var resourceName = "ALoRa.UnitTests.sampleMessage.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentNullException(resourceName);
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}