using EletrocaoMauiApp.Services;

namespace EletrocaoMauiApp.Views;

public partial class ConexaoPage : ContentPage
{
	private readonly IHiveMqService _hiveMqService;
	private readonly IConfiguracoesDasJuntasService _juntasService;
	private static bool _topicosJaSubscritos = false;

	public ConexaoPage(IHiveMqService hiveMqService, IConfiguracoesDasJuntasService servomotorService)
	{
		InitializeComponent();
		_hiveMqService = hiveMqService;
		_juntasService = servomotorService;
		this.BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (Preferences.ContainsKey("Usuario") && Preferences.ContainsKey("Senha"))
		{
			usuarioEntry.Text = Preferences.Get("Usuario", "");
			senhaEntry.Text = Preferences.Get("Senha", "");
			salvarDadosLogin_Check.IsChecked = true;
		}
	}

	private async void Connection_Btn_Clicked(object sender, EventArgs e)
	{

		Connection_Btn.IsEnabled = false;

		indicadorDeAtividade.IsRunning = true;

		var usuario = usuarioEntry.Text;
		var senha = senhaEntry.Text;

		string host = Preferences.Get("Host", "");
		int port = Preferences.Get("Port", 0000);

		_hiveMqService.ConfigurarCliente(usuario, senha, host, port);

		bool connectionStatus = await _hiveMqService.ConnectClientAsync();

		if (connectionStatus)
		{
			Preferences.Set("ConectadoMq", "Sim");
			if (salvarDadosLogin_Check.IsChecked)
			{
				Preferences.Set("Usuario", usuario);
				Preferences.Set("Senha", senha);
			}
			else
			{
				Preferences.Remove(usuario);
				Preferences.Remove(senha);
			}
			SubscribirTopicosSeNecessario();

			indicadorDeAtividade.IsRunning = false;
			Connection_Btn.IsEnabled = true;
			await Navigation.PushAsync(new ControleDoRoboPage(_hiveMqService, _juntasService));

		}
		else
		{
			await DisplayAlert("Erro", "Falha ao conectar!", "Voltar");
			indicadorDeAtividade.IsRunning = false;
			Connection_Btn.IsEnabled = true;
		}
	}

	private void SubscribirTopicosSeNecessario()
	{
		if (_topicosJaSubscritos)
			return;

		try
		{
			 _hiveMqService.Subscribe(Constantes.Topicos.TopicoParaInformacoesDoRobo);

			_topicosJaSubscritos = true;
			System.Diagnostics.Debug.WriteLine("Subscrições MQTT concluídas com sucesso!");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Erro ao subscrever tópicos: {ex.Message}");
		}
	}

	private async void NavegarParaPreferenciasDoMqtt(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new PreferenciasPage());
	}

	private void FecharAplicativo(object sender, EventArgs e)
	{
		Application.Current?.Quit();
	}
}