using System;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ALoRa.Library;

public class TtnApplication : BaseObject
{
    private const string BrokerUrlFilter = "{0}.thethings.network";

    private readonly MqttClient _client;
    private Action<TtnMessage>? _msgReceived;

    public TtnApplication(string appId, string accessKey, string region)
    {
        AppId = appId;
        var clientId = Guid.NewGuid().ToString();

        var host = string.Format(BrokerUrlFilter, region);
        _client = new MqttClient(host);
        _client.MqttMsgPublishReceived += M_client_MqttMsgPublishReceived;
        _client.ConnectionClosed += M_client_ConnectionClosed;
        _client.MqttMsgSubscribed += M_client_MqttMsgSubscribed;
        _client.MqttMsgUnsubscribed += M_client_MqttMsgUnsubscribed;

        _client.Connect(clientId, AppId, accessKey);

        _client.Subscribe(new[] { "#" }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    public string AppId { get; }

    public bool IsConnected => _client.IsConnected;

    public event Action<TtnMessage> MessageReceived
    {
        add => _msgReceived += value;
        remove => _msgReceived -= value;
    }

    protected override void Dispose(bool disposing)
    {
        _client.Disconnect();
    }

    public void Publish()
    {
        //m_client.Publish()
    }

    private void M_client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Console.WriteLine("M_client_MqttMsgPublishReceived");
        try
        {
            var msg = TtnMessage.DeserializeMessage(e.Message, e.Topic);

            if (_msgReceived != null)
            {
                _msgReceived(msg);
            }
        }
        catch
        {
            // Swallow any exceptions during message receive
        }
    }

    private void M_client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
    {
        Console.WriteLine("M_client_MqttMsgUnsubscribed");
    }

    private void M_client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
    {
        Console.WriteLine("M_client_MqttMsgSubscribed");
    }

    private void M_client_ConnectionClosed(object sender, EventArgs e)
    {
        Console.WriteLine("M_client_ConnectionClosed");
    }
}