using EletrocaoMauiApp.Resources.Strings;
using EletrocaoMauiApp.Services;
using EletrocaoMauiApp.Views;
using System.Globalization;
namespace EletrocaoMauiApp;

public partial class App : Application
{
	private readonly IHiveMqService _hiveMqService;
	private readonly IConfiguracoesDasJuntasService _configuracoesDasJuntasService;
	public App(IHiveMqService hiveMqService, IConfiguracoesDasJuntasService servomotorService)
	{
		InitializeComponent();
		_configuracoesDasJuntasService = servomotorService;
		_hiveMqService = hiveMqService;
		SetMainPage();
		_ = InicializarAsync();
	}

	private async Task InicializarAsync()
	{
		var sequenciaDePosturasJson = new Utilitarios.SequenciaDePosturasJson();
		await sequenciaDePosturasJson.CopiarArquivosDeRawParaAppDataAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(MainPage)
		{
			Width = 400,
			Height = 780,
			X = 0,
			Y = 0
		};
	}


	public static void SetCulture(string culture)
	{
		Preferences.Set("Culture", culture);
		var ci = new CultureInfo(culture);
		Thread.CurrentThread.CurrentUICulture = ci;
		Thread.CurrentThread.CurrentCulture = ci;
		AppResources.Culture = ci;
	}
	private void SetMainPage()
	{
		MainPage = new NavigationPage(new ConexaoPage(_hiveMqService, _configuracoesDasJuntasService));
	}
}
