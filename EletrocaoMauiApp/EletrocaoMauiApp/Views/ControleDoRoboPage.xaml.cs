using Constantes;
using EletrocaoMauiApp.Mappers;
using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Resources.Strings;
using EletrocaoMauiApp.Services;
using NLog;
using System.Net.Sockets;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class ControleDoRoboPage : ContentPage
{
	private readonly IHiveMqService _mqService;
	private readonly IConfiguracoesDasJuntasService _configuracoesDasJuntasService;
	private bool _isPressed;
	private CancellationTokenSource _cts;
	private Dictionary<string, Action<string>> topicHandlers = new Dictionary<string, Action<string>>();

	public ControleDoRoboPage(IHiveMqService mqService, IConfiguracoesDasJuntasService configuracoesDasJuntasService)
	{
		InitializeComponent();
		_mqService = mqService;
		_configuracoesDasJuntasService = configuracoesDasJuntasService;
		topicHandlers.Add(Constantes.Topicos.TopicoParaInformacoesDoRobo, HandleInformacoesRobo);
		this.IsBusy = false;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_mqService.MessageReceived += OnMessageReceived;
	}
	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Garante que o evento não continue ativo se a página for destruída
		_mqService.MessageReceived -= OnMessageReceived;
	}


	private async void OnMessageReceived(string topic, string message)
	{
		if (topicHandlers.TryGetValue(topic, out var handler))
		{
			handler(message); // Chama o handler correto
		}
		else
		{
			Console.WriteLine($"Nenhum handler para o tópico: {topic}");
		}
	}
	private async void HandleInformacoesRobo(string message)
	{
		try
		{
			var informacaoDoRobo = JsonSerializer.Deserialize<InformacoesDoRobo>(
				message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
);

			if (informacaoDoRobo == null) return;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				AtualizarInformacoesDoRobo(informacaoDoRobo);
			});
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Erro ao desserilizar as informações do robô: {ex.Message}", "Ok");
		}
	}

	private void AtualizarInformacoesDoRobo(InformacoesDoRobo informacoesDoRobo)
	{
		string estado = informacoesDoRobo.Estado;
		double tensaoBateriaServomotores = informacoesDoRobo.TensaoBateriaServomotores;
		double tensaoBateriaMicro = informacoesDoRobo.TensaoBateriaMicro;
		double corrente = informacoesDoRobo.Corrente * 1000;


		EstadoRoboLabel.Text = AppResources.CONECTADO;
		if (estado == "Conectado")
			EstadoRobo.Fill = Colors.Green;
		else
			EstadoRobo.Fill = Colors.Red;


		if (tensaoBateriaServomotores >= 10 && tensaoBateriaServomotores <= 12.5)
			TensaoBateriaServomotoresLabel.Text = $"{tensaoBateriaServomotores:F2} V \U0001F50B";
		else if (tensaoBateriaServomotores > 12.5 || tensaoBateriaServomotores < 9.3)
			TensaoBateriaServomotoresLabel.Text = $"{tensaoBateriaServomotores:F2} V \u26A0\uFE0F";
		else if (tensaoBateriaServomotores < 10 && tensaoBateriaServomotores >= 9.3)
			TensaoBateriaServomotoresLabel.Text = $"{tensaoBateriaServomotores:F2} V \U0001FAAB";

		if (tensaoBateriaMicro >= 3.5 && tensaoBateriaMicro <= 4.4)
			TensaoBateriaMicroLabel.Text = $"{tensaoBateriaMicro:F2} V \U0001F50B";
		else if (tensaoBateriaMicro > 4.4 || tensaoBateriaMicro < 3.1)
			TensaoBateriaMicroLabel.Text = $"{tensaoBateriaMicro:F2} V \u26A0\uFE0F";
		else if (tensaoBateriaMicro < 3.5 && tensaoBateriaMicro >= 3.1)
			TensaoBateriaMicroLabel.Text = $"{tensaoBateriaMicro:F2} V \U0001FAAB";

		if (corrente >= 1000)
			CorrenteLabel.Text = $"{corrente:F0} mA \u26A0\uFE0F";
		else
			CorrenteLabel.Text = $"{corrente:F0} mA";
	}

	private async void OnComandoPressed(object sender, EventArgs e)
	{


		long timestamp = await NPT.GetNtpUnixTimeMs();

		if (sender is not Button button || button.CommandParameter is not string arquivoJson)
			return;

		if (sender is Button botao)
		{
			botao.BackgroundColor = Colors.RoyalBlue; // cor ao pressionar
		}

		IEnumerable<ConfiguracoesDaJunta> configuracoesDasJuntas =
			await _configuracoesDasJuntasService.GetAllConfiguracoesDasJuntasAsync();

		_isPressed = true;
		_cts = new CancellationTokenSource();

		try
		{
			var sequencia = await CarregarSequenciaAsync(arquivoJson);
			if (sequencia is null)
				return;
			var msgIniciar = JsonSerializer.Serialize(new { Nome = "Iniciar" });//Instrução para iniciar a sequência
			_mqService.Publish(msgIniciar, Constantes.Topicos.TopicoParaInstrucoes);
			// envia imediatamente
			EnviarComando(sequencia, configuracoesDasJuntas, timestamp);

			// envia repetidamente enquanto pressionado
			while (_isPressed && !_cts.Token.IsCancellationRequested)
			{
				await Task.Delay(1000, _cts.Token);

				if (_isPressed)
				{
					var msg = JsonSerializer.Serialize(new { Nome = sequencia.Nome }); //Instrução para repetir a sequência
					_mqService.Publish(msg, Constantes.Topicos.TopicoParaInstrucoes);
				}
			}
		}
		catch (TaskCanceledException)
		{
			// ignore cancelamento normal
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Não foi possível carregar o comando: {ex.Message}", "OK");
		}
	}

	private void OnComandoReleased(object sender, EventArgs e)
	{
		_isPressed = false;
		_cts?.Cancel();

		if (sender is Button botao)
		{
			botao.BackgroundColor = Colors.WhiteSmoke; // volta para cor original
		}

		// Envia mensagem de parada
		var msgParar = JsonSerializer.Serialize(new { Nome = "Parar" }); //Instrução para parar a sequência
		_mqService.Publish(msgParar, Constantes.Topicos.TopicoParaInstrucoes);
	}

	private async Task<SequenciaDePosturas?> CarregarSequenciaAsync(string arquivoJson)
	{
		// Caminho completo do arquivo na pasta local do app
		var caminho = Path.Combine(FileSystem.AppDataDirectory, arquivoJson);

		if (!File.Exists(caminho))
			return null; // ou lançar exceção se preferir

		var conteudo = await File.ReadAllTextAsync(caminho);

		return JsonSerializer.Deserialize<SequenciaDePosturas>(
			conteudo,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
	}

	private void EnviarComando(SequenciaDePosturas sequencia, IEnumerable<ConfiguracoesDaJunta> configuracoesDasJuntas, long timeStamp)
	{
		(SequenciaDeComandosDasJuntas, SequenciaDeComandosDasJuntasEsp32) sequenciaDeComandosParaEsp32ESimulador = sequencia.MapearParaSequenciaDeComandos(configuracoesDasJuntas, timeStamp);

		var sequenciaDeComandos = JsonSerializer.Serialize(sequenciaDeComandosParaEsp32ESimulador.Item1);
		var sequenciaDeComandosEsp32 = JsonSerializer.Serialize(sequenciaDeComandosParaEsp32ESimulador.Item2);

		_mqService.Publish(sequenciaDeComandos, Constantes.Topicos.TopicoParaControlePorSequenciaDeComandos);
		_mqService.Publish(sequenciaDeComandosEsp32, Constantes.Topicos.TopicoParaControlePorSequenciaDeComandosEsp32);
	}

	private async void ConectarAoSimulador_Clicked(object sender, EventArgs e)
	{
		var configs = await _configuracoesDasJuntasService.GetAllConfiguracoesDasJuntasAsync();
		if (configs == null || configs.Count == 0)
		{
			await DisplayAlert("Erro", "Nenhuma configuração de junta encontrada.", "OK");
			return;
		}

		var configsParaOSimulador = configs.MapearParaSimulador();
		var msg = JsonSerializer.Serialize(configsParaOSimulador);

		_mqService.Publish(msg, Constantes.Topicos.TopicoParaConfigurarAsJuntasDoCoppeliaSim);
		EstadoSimulador.Fill = Colors.Green;
		EstadoDoSimuladorLabel.Text = AppResources.CONECTADO;
	}

	private async void NavegarParaInformacoesDoRobo(object sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new InformacoesDoRoboPage(_mqService));
	}

	private async void NavegarParaJuntas(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new JuntasPage(_mqService, _configuracoesDasJuntasService));
	}
	private async void NavegarParaControleDasJuntas(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new ControleDasJuntasPage(_mqService, _configuracoesDasJuntasService));
	}

	private async void NavegarParaConexao(object sender, EventArgs e)
	{
		var sair = await DisplayAlert("Desconectar", "Deseja voltar para a página de conexão?", "Ok", "Cancelar");

		if (!sair)
			return;
		indicadorDeAtividade.IsRunning = true;

		Preferences.Set("ConectadoMq", "Não");
		var desconectou = await _mqService.DisconnectClientAsync();
		if (desconectou)
		{
			indicadorDeAtividade.IsRunning = false;
			await Navigation.PushAsync(new ConexaoPage(_mqService, _configuracoesDasJuntasService));
		}
		else
		{
			await DisplayAlert("Erro", "Algo ocorreu de errado", "Ok");
		}
	}

	private async void IrParaSequenciaDePosturas(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SequenciaDePosturasPage());
	}


}