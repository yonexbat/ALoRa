using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ALoRa.Library;

public class TtnApplication : BaseObject
{
    private const string BrokerUrlFilter = "{0}.thethings.network";

    private readonly MqttClient _client;
    private Action<TtnMessage>? _msgReceived;
    private readonly ILogger _logger;

    public TtnApplication(string appId, string accessKey, string region, ILogger<TtnApplication> logger)
    {
        _logger = logger;
        AppId = appId;
        var clientId = Guid.NewGuid().ToString();

        var host = string.Format(BrokerUrlFilter, region);
        _client = new MqttClient(host);
        _client.MqttMsgPublishReceived += PublishReceived;
        _client.ConnectionClosed += ConnectionClosed;
        _client.MqttMsgSubscribed += Subscribed;
        _client.MqttMsgUnsubscribed += Unsubscribed;

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

    public void Publish(string device, byte[] payload)
    {
        DownLinkMessage message = new DownLinkMessage
        {
            DownLinks = new List<DownLink>()
            {
                new DownLink
                {
                    FrmPayload = "vu8=",
                    Priority = "NORMAL",
                    FPort = 15,
                    CorrelationIds = new List<string>(){ Guid.NewGuid().ToString()}
                }
            } 
        };
        string mesageAsString = JsonSerializer.Serialize(message);
        byte[] messageAsBytes = Encoding.UTF8.GetBytes(mesageAsString);


        _client.Publish($"v3/{AppId}/devices/{device}/down/push", messageAsBytes);

        //m_client.Publish()
    }

    private void PublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        _logger.LogInformation("MqttMsgPublishReceived event. Topic: {Topic}. ", e.Topic);
        try
        {
            var msg = TtnMessage.DeserializeMessage(e.Message, e.Topic);
            _msgReceived?.Invoke(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private void Unsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
    {
        _logger.LogInformation("MqttMsgUnsubscribed event");
    }

    private void Subscribed(object sender, MqttMsgSubscribedEventArgs e)
    {
        _logger.LogInformation("MqttMsgSubscribed event");
    }

    private void ConnectionClosed(object sender, EventArgs e)
    {
        _logger.LogInformation("ConnectionClosed event");
    }
}