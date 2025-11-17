using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class CarregarJuntasPage : ContentPage
{
	private IHiveMqService _mqService;
	private IConfiguracoesDasJuntasService _configuracoesDasJuntasService;
	private bool _receber = false;
	public CarregarJuntasPage(IHiveMqService mqService, IConfiguracoesDasJuntasService configuracoesDasJuntasService)
	{
		InitializeComponent();
		_configuracoesDasJuntasService = configuracoesDasJuntasService;
		_mqService = mqService;
		_mqService.SubscribeAsync("carga");
		_mqService.MessageReceived += OnMessageReceived;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		_mqService.MessageReceived -= OnMessageReceived;
	}

	private async void OnMessageReceived(string message)
	{
		if(_receber)
		{
			try
			{
				// Desserializa a string JSON para um objeto TelemetryData
				var juntas = JsonSerializer.Deserialize<IEnumerable<ConfiguracoesDaJunta>>(message);
				if (juntas == null)
					return;

				foreach (ConfiguracoesDaJunta junta in juntas)
				{
					junta.Id = 0; // força o SQLite a gerar novo Id
					await _configuracoesDasJuntasService.CreateConfiguracoesDasJuntasAsync(junta);
				}
				_receber = false;

				MainThread.BeginInvokeOnMainThread(async () =>
				{
					indicadorDeAtividadeCarregar.IsRunning = _receber;
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

	private async  void Enviar_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Desserializa a string JSON para um objeto TelemetryData
			var json = JsonSerializer.Serialize(await _configuracoesDasJuntasService.GetAllConfiguracoesDasJuntasAsync());
			_mqService.Publish(json, "carga");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Erro ao desserializar JSON: {ex.Message}", "Ok");
		}


	}

	private void Receber_Clicked(object sender, EventArgs e)
	{
		
		_receber = _receber == true ? false : true;
		Enviar.IsEnabled = !_receber;
		indicadorDeAtividadeCarregar.IsRunning = _receber;
	}

	private async void NavegarParaJuntas(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}