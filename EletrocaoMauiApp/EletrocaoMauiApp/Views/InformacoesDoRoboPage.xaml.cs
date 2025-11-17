using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;
using SkiaSharp;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class InformacoesDoRoboPage : ContentPage
{
	private readonly IHiveMqService _mqService;
	private readonly List<PontoGrafico> _tensoesBateriaServomotores = new();
	private readonly List<PontoGrafico> _tensoesBateriaMicro = new();
	private readonly List<PontoGrafico> _correntes = new();
	private readonly List<PontoGrafico> _contagemDePacotes = new();
	private const int numeroDePontos = 15;
	private bool graficosForamCriados = false;
	private int _mensagensPerdidas = 0;
	private int _ultimaMensagemRecebida = 0;
	private int _mensagensRecebidas = 0;

	public InformacoesDoRoboPage(IHiveMqService mqService)
	{
		InitializeComponent();

		_mqService = mqService;
		_mqService.MessageReceived += OnMessageReceived;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Garante que o evento não continue ativo se a página for destruída
		_mqService.MessageReceived -= OnMessageReceived;
	}

	private async void OnMessageReceived(string message)
	{
		try
		{
			var informacaoDoRobo = JsonSerializer.Deserialize<InformacoesDoRobo>(
				message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
);

			if (informacaoDoRobo == null) return;

			VerificarPerdaDePacotes(informacaoDoRobo.ContadorDeMensagens);

			MainThread.BeginInvokeOnMainThread(() =>
			{
				AtualizarEstadoDoRobo(informacaoDoRobo.Estado, informacaoDoRobo.Mensagem);

				AtualizarGraficoTensaoBateriaServomotores((float)informacaoDoRobo.TensaoBateriaServomotores, informacaoDoRobo.Timestamp);
				AtualizarGraficoTensaoBateriaMicro((float)informacaoDoRobo.TensaoBateriaMicro, informacaoDoRobo.Timestamp);
				AtualizarGraficoCorrente((float)informacaoDoRobo.Corrente * -15, informacaoDoRobo.Timestamp);
				AtualizarGraficoContagemDePacotes(informacaoDoRobo.ContadorDeMensagens, informacaoDoRobo.Timestamp);

			});
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Erro ao desserializar JSON: {ex.Message}", "Ok");
		}
	}

	private void VerificarPerdaDePacotes(int contagemDeMensagens)
	{
		if (_ultimaMensagemRecebida == 0)
			_ultimaMensagemRecebida = contagemDeMensagens;

		int diferencaUltimaMensagem = contagemDeMensagens - _ultimaMensagemRecebida;
		if (diferencaUltimaMensagem < 1)
			return;
		if(diferencaUltimaMensagem > 1)
			_mensagensPerdidas += diferencaUltimaMensagem - 1;


		_ultimaMensagemRecebida = contagemDeMensagens;
		_mensagensRecebidas++;
		var taxaDeRecebimento = (_mensagensRecebidas / (double)(_mensagensRecebidas + _mensagensPerdidas))* 100;

		MainThread.BeginInvokeOnMainThread(async () =>
		{
			FrameMensagensRecebidas.IsVisible = true;
			MensagensRecebidosLabel.Text = $"Mensagens recebidas: {_mensagensRecebidas}";
			MensagensPerdidasLabel.Text = $"Mensagens perdidas: {_mensagensPerdidas}";
			TaxaDeRecebimentoLabel.Text = $"Taxa de recebimento: {taxaDeRecebimento.ToString("F2")}%";
		});
	}


	private void AtualizarEstadoDoRobo(string estado, string mensagem)
	{
		if (string.IsNullOrWhiteSpace(estado))
			EstadoDoRobo_Lbl.Text = "Estado indefinido";
		else
			EstadoDoRobo_Lbl.Text = estado;
		if (!string.IsNullOrEmpty(mensagem))
		{
			Mensagem_Lbl.Text = mensagem;
			FrameMensagem.IsVisible = true;
		}
	}

	private void AtualizarGraficoTensaoBateriaServomotores(float valor, TimeSpan timestamp)
	{
		_tensoesBateriaServomotores.Add(new PontoGrafico { Valor = valor, Timestamp = timestamp });

		if (_tensoesBateriaServomotores.Count < numeroDePontos / 5)
			return;

		if (_tensoesBateriaServomotores.Count > numeroDePontos)
			_tensoesBateriaServomotores.RemoveAt(0);

		LabelGraficoTensaoBateriaServomotores.Text = "Tensão da bateria dos servomotores (V)";
		TensaoBateriaServomotoresChart.IsVisible = true;

		var entries = _tensoesBateriaServomotores.Select(p => new Microcharts.ChartEntry(p.Valor)
		{
			Label = p.Timestamp.ToString(@"hh\:mm\:ss"), // eixo X com timestamp
			ValueLabel = p.Valor.ToString("F2"),
			Color = SKColor.Parse("#007ACC")
		}).ToList();

		var novoChart = new Microcharts.Maui.ChartView
		{
			Chart = new Microcharts.LineChart
			{
				Entries = entries,
				LineMode = Microcharts.LineMode.Straight,
				LineSize = 6,
				PointSize = 12,
				LabelTextSize = 20,
				ValueLabelTextSize = 18,
				BackgroundColor = SKColors.Transparent,
				IsAnimated = false
			},
			HeightRequest = 200
		};

		var parent = (VerticalStackLayout)TensaoBateriaServomotoresChart.Parent;
		int index = parent.Children.IndexOf(TensaoBateriaServomotoresChart);
		parent.Children.RemoveAt(index);
		parent.Children.Insert(index, novoChart);

		TensaoBateriaServomotoresChart = novoChart;
	}


	private void AtualizarGraficoTensaoBateriaMicro(float valor, TimeSpan timestamp)
	{
		_tensoesBateriaMicro.Add(new PontoGrafico { Valor = valor, Timestamp = timestamp });

		if (_tensoesBateriaMicro.Count < numeroDePontos / 5)
			return;

		if (_tensoesBateriaMicro.Count > numeroDePontos)
			_tensoesBateriaMicro.RemoveAt(0);

		LabelGraficoTensaoBateriaMicro.Text = "Tensão da bateria do microcontrolador (V)";
		TensaoBateriaMicroChart.IsVisible = true;

		var entries = _tensoesBateriaMicro.Select(p => new Microcharts.ChartEntry(p.Valor)
		{
			Label = p.Timestamp.ToString(@"hh\:mm\:ss"), // eixo X com timestamp
			ValueLabel = p.Valor.ToString("F2"),
			Color = SKColor.Parse("#007ACC")
		}).ToList();

		var novoChart = new Microcharts.Maui.ChartView
		{
			Chart = new Microcharts.LineChart
			{
				Entries = entries,
				LineMode = Microcharts.LineMode.Straight,
				LineSize = 6,
				PointSize = 12,
				LabelTextSize = 20,
				ValueLabelTextSize = 18,
				BackgroundColor = SKColors.Transparent,
				IsAnimated = false
			},
			HeightRequest = 200
		};

		var parent = (VerticalStackLayout)TensaoBateriaMicroChart.Parent;
		int index = parent.Children.IndexOf(TensaoBateriaMicroChart);
		parent.Children.RemoveAt(index);
		parent.Children.Insert(index, novoChart);

		TensaoBateriaMicroChart = novoChart;
	}

	private void AtualizarGraficoCorrente(float valor, TimeSpan timestamp)
	{


		_correntes.Add(new PontoGrafico { Valor = valor, Timestamp = timestamp });

		if (_correntes.Count < numeroDePontos / 5)
			return;
		if (_correntes.Count > numeroDePontos)
			_correntes.RemoveAt(0);

		LabelGraficoCorrente.Text = "Corrente (mA)";
		CorrenteChart.IsVisible = true;
		var entries = _correntes.Select(p => new Microcharts.ChartEntry(p.Valor)
		{
			Label = p.Timestamp.ToString(@"hh\:mm\:ss"),
			ValueLabel = p.Valor.ToString("F0"),
			Color = SKColor.Parse("#FF4500")
		}).ToList();

		var novoChart = new Microcharts.Maui.ChartView
		{
			Chart = new Microcharts.LineChart
			{
				Entries = entries,
				LineMode = Microcharts.LineMode.Straight,
				LineSize = 6,
				PointSize = 8,
				LabelTextSize = 20,
				ValueLabelTextSize = 18,
				BackgroundColor = SKColors.Transparent,
				IsAnimated = false
			},
			HeightRequest = 200
		};

		var parent = (VerticalStackLayout)CorrenteChart.Parent;
		int index = parent.Children.IndexOf(CorrenteChart);
		parent.Children.RemoveAt(index);
		parent.Children.Insert(index, novoChart);

		CorrenteChart = novoChart;
	}

	private void AtualizarGraficoContagemDePacotes(int valor, TimeSpan timestamp)
	{
		_contagemDePacotes.Add(new PontoGrafico { Valor = valor, Timestamp = timestamp });

		if (_contagemDePacotes.Count < numeroDePontos / 5)
			return;

		if (_contagemDePacotes.Count > numeroDePontos)
			_contagemDePacotes.RemoveAt(0);

		LabelGraficoContagemDePacotes.Text = "Contagem de Pacotes";
		ContagemDePacotesChart.IsVisible = true;

		var entries = _contagemDePacotes.Select(p => new Microcharts.ChartEntry(p.Valor)
		{
			Label = p.Timestamp.ToString(@"hh\:mm\:ss"),
			ValueLabel = p.Valor.ToString("F0"),
			Color = SKColor.Parse("#32CD32") // verde para diferenciar
		}).ToList();

		var novoChart = new Microcharts.Maui.ChartView
		{
			Chart = new Microcharts.LineChart
			{
				Entries = entries,
				LineMode = Microcharts.LineMode.Straight,
				LineSize = 6,
				PointSize = 10,
				LabelTextSize = 20,
				ValueLabelTextSize = 18,
				BackgroundColor = SKColors.Transparent,
				IsAnimated = false
			},
			HeightRequest = 200
		};

		var parent = (VerticalStackLayout)ContagemDePacotesChart.Parent;
		int index = parent.Children.IndexOf(ContagemDePacotesChart);
		parent.Children.RemoveAt(index);
		parent.Children.Insert(index, novoChart);

		ContagemDePacotesChart = novoChart;
	}

	private async void Voltar_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}
