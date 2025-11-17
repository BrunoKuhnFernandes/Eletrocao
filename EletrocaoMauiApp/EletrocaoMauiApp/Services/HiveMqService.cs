using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

namespace EletrocaoMauiApp.Services;
public interface IHiveMqService
{
	HiveMQClient ConfigurarCliente(string usuario, string senha, string host, int port);
	Task<bool> ConnectClientAsync();
	Task<bool> DisconnectClientAsync();
	void Publish(string message, string topic);
	void Subscribe(string topic);
	event Action<string>? MessageReceived;
}

public class HiveMqService : IHiveMqService
{
	private HiveMQClient? _client;
	private bool _useTLS = true;

	public HiveMQClient ConfigurarCliente(string usuario, string senha, string host, int port)
	{
		var options = new HiveMQClientOptions
		{
			Host = host,
			Port = port,
			UseTLS = _useTLS,
			UserName = usuario,
			Password = senha,
		};
		return _client = new HiveMQClient(options);
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
			return false; //TODO: Retornar erro para exibir no display alert.
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
			// TODO: Log do erro ou exibição de alerta para o usuário
			return false;
		}
	}

	public async void Publish(string message, string topic)
	{
		//Publish MQTT messages
		var result = await _client.PublishAsync(topic, message, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
	}

	public event Action<string>? MessageReceived; // Evento para notificar a TelemetryPage

	public async void Subscribe(string topic)
	{
		_client.OnMessageReceived += (sender, args) =>
		{
			string message = args.PublishMessage.PayloadAsString;
			MessageReceived?.Invoke(message);
		};

		var builder = new SubscribeOptionsBuilder();
		builder.WithSubscription(topic, QualityOfService.AtLeastOnceDelivery);
		var subscribeOptions = builder.Build();
		await _client.SubscribeAsync(subscribeOptions);
	}

}
