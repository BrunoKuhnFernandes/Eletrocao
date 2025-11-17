using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class CarregarCfgDasJuntasPage : ContentPage
{
	private readonly IHiveMqService _mqService;
	private readonly IConfiguracoesDasJuntasService _configuracoesDasJuntasService;
	private bool _carregar = false;

	public CarregarCfgDasJuntasPage(IHiveMqService mqService, IConfiguracoesDasJuntasService configuracoesDasJuntasService)
	{
		InitializeComponent();
		_configuracoesDasJuntasService = configuracoesDasJuntasService;
		_mqService = mqService;

		// Assina o tópico
		_mqService.Subscribe(Constantes.Topicos.TopicoParaTransferirConfiguracoesDasJuntas);
		_mqService.MessageReceived += OnMessageReceived;

		this.BindingContext = this;
	}

	private async void OnMessageReceived(string message)
	{
		if (_carregar)
		{
			try
			{
				var juntas = JsonSerializer.Deserialize<IEnumerable<ConfiguracoesDaJunta>>(message);
				if (juntas == null)
					return;

				foreach (var junta in juntas)
				{
					junta.Id = 0; // força o SQLite a gerar novo Id
					await _configuracoesDasJuntasService.CreateConfiguracoesDasJuntasAsync(junta);
				}

				_carregar = false;

				MainThread.BeginInvokeOnMainThread(async () =>
				{
					indicadorDeAtividadeCarregar.IsRunning = false;
					await DisplayAlert("Sucesso", "Configurações recebidas com sucesso!", "OK");
					await Navigation.PopAsync();
				});
			}
			catch (Exception ex)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await DisplayAlert("Erro", $"Falha ao carregar configurações: {ex.Message}", "OK");
				});
			}
		}
	}

	private async void Enviar_Clicked(object sender, EventArgs e)
	{
		try
		{
			var json = JsonSerializer.Serialize(
				await _configuracoesDasJuntasService.GetAllConfiguracoesDasJuntasAsync()
			);
			_mqService.Publish(json, Constantes.Topicos.TopicoParaTransferirConfiguracoesDasJuntas);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Erro ao enviar configurações: {ex.Message}", "Ok");
		}
	}

	private void Receber_Clicked(object sender, EventArgs e)
	{
		Receber_Btn.Text = _carregar == false ? "Cancelar":"Receber";
		_carregar = !_carregar;
		Enviar_Btn.IsEnabled = !_carregar;
		indicadorDeAtividadeCarregar.IsRunning = _carregar;
	}

	private async void NavegarParaJuntas(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Garante que o evento não continue ativo se a página for destruída
		_mqService.MessageReceived -= OnMessageReceived;
	}
}
