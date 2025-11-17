using EletrocaoMauiApp.Services;

namespace EletrocaoMauiApp.Views;

public partial class MainPage : ContentPage
{
	private IHiveMqService _mqService;
	private readonly IConfiguracoesDasJuntasService _configuracoesDasJuntasService;
	public MainPage(IHiveMqService hiveMqService, IConfiguracoesDasJuntasService servomotorService)
	{
		_mqService = hiveMqService;
		_configuracoesDasJuntasService = servomotorService;
		InitializeComponent();
	}


	private async void NavegarParaInformacoesDoRobo(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new InformacoesDoRoboPage(_mqService));
	}

	private async void Desconectar_Btn_Clicked(object sender, EventArgs e)
	{
		Preferences.Set("ConectadoMq", "Não");
		var desconectou = await _mqService.DisconnectClientAsync();
		if (desconectou)
		{
			await DisplayAlert("Desconectado","Voltar para a página de conexão","Ok");
			await Navigation.PushAsync(new ConexaoPage(_mqService, _configuracoesDasJuntasService));
		}
		else {
			await DisplayAlert("Erro", "Algo ocorreu de errado", "Ok");
		}
	}

	private async void NavegarParaConfiguracoesDasJuntas(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new JuntasPage(_mqService, _configuracoesDasJuntasService));
	}

	private async void NavegarParaControleDoRobo(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new ControleDoRoboPage(_mqService, _configuracoesDasJuntasService));
	}
}

