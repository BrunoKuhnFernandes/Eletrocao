using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using System.Text.Json;

namespace CoppeliaApiCS.Services;
public interface IHiveMqService
{
	Task<bool> ConnectClientAsync();
	Task<bool> DisconnectClientAsync();
	void PublishAsync(string message, string topic);
	void SubscribeAsync(string topic);
	event Action<string, string>? MessageReceived;
}

public class HiveMqService : IHiveMqService
{
	private HiveMQClient _client;
	private string _host = "aee98b038657422eaa153dc16db4cd55.s1.eu.hivemq.cloud";
	private int _port = 8883;
	private bool _useTLS = true;

	public HiveMqService(string usuario, string senha)
	{
		var options = new HiveMQClientOptions
		{
			Host = _host,
			Port = _port,
			UseTLS = _useTLS,
			UserName = usuario,
			Password = senha,
		};
		_client = new HiveMQClient(options);
	}

	public async Task<bool> ConnectClientAsync()
	{
		try
		{
			HiveMQtt.Client.Results.ConnectResult connectResult = await _client.ConnectAsync().ConfigureAwait(false);
			return connectResult.ReasonCode == ConnAckReasonCode.Success;
		}
		catch (System.Net.Sockets.SocketException e)
		{
			return false;
		}
		catch (Exception e)
		{
			return false;
		}
	}

	public async Task<bool> DisconnectClientAsync()
	{
		try
		{
			if (_client.IsConnected())
			{
				await _client.DisconnectAsync().ConfigureAwait(false);
				return true;
			}
			return true;
		}
		catch (Exception e)
		{
			return false;
		}
	}

	public async void PublishAsync(string message, string topic)
	{
		var result = await _client.PublishAsync(topic, message, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
	}

	public event Action<string, string>? MessageReceived;

	public void PrepareClientToSubscribe()
	{
		_client.OnMessageReceived += (sender, args) =>
		{
			string topic = args.PublishMessage.Topic;
			string message = args.PublishMessage.PayloadAsString;
			MessageReceived?.Invoke(topic, message);
		};
	}
	public async void SubscribeAsync(string topic)
	{
		var builder = new SubscribeOptionsBuilder();
		builder.WithSubscription(topic, QualityOfService.AtLeastOnceDelivery);
		var subscribeOptions = builder.Build();
		await _client.SubscribeAsync(subscribeOptions);
	}
}