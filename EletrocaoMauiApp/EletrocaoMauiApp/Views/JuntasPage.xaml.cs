using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;


namespace EletrocaoMauiApp.Views;

public partial class JuntasPage : ContentPage
{
	private readonly IConfiguracoesDasJuntasService _dbService;
	private readonly IHiveMqService _hiveMqService;
	private int _juntasCount { get; set; }
	public JuntasPage(IHiveMqService hiveMqService, IConfiguracoesDasJuntasService dbService)
	{
		_hiveMqService = hiveMqService;
		_dbService = dbService;
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await GetJuntas();
	}

	private async Task GetJuntas()
	{
		try
		{
			var juntas = await _dbService.GetAllConfiguracoesDasJuntasAsync();
			_juntasCount = juntas.Count;

			if (juntas is null || _juntasCount == 0)
			{
				CvJuntas.ItemsSource = null;
				LblAviso.IsVisible = true;
			}
			else
			{
				CvJuntas.ItemsSource = juntas;
				LblAviso.IsVisible = false;
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
		}
	}

	private async void CriarJunta_Clicked(object sender, EventArgs e)
	{
		var junta = new ConfiguracoesDaJunta
		{
			Nome = $"Junta {++_juntasCount}"
		};
		
		await _dbService.CreateConfiguracoesDasJuntasAsync(junta);

		await Navigation.PushAsync(new ConfiguracaoDaJuntaPage(junta.Id, _dbService));
	}

	private async void VoltarParaPaginaDeControle(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private async void CarregarConfiguracoes(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CarregarCfgDasJuntasPage(_hiveMqService, _dbService));
	}

	private async void OnJunta_Tapped(object sender, TappedEventArgs e)
	{
		// sender é o Grid ou Frame que recebeu o toque
		var grid = sender as Grid; // ou Frame, dependendo do que você usou
		if (grid == null) return;

		// Obtemos o BindingContext do item
		if (grid.BindingContext is ConfiguracoesDaJunta junta)
		{
			await Navigation.PushAsync(new ConfiguracaoDaJuntaPage(junta.Id, _dbService));
		}
	}
}
