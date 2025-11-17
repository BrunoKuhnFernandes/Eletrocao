using EletrocaoMauiApp.Services;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace EletrocaoMauiApp;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMicrocharts()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "O3penSansSemibold");
				fonts.AddFont("Jost-Regular.ttf", "JostRegular");
			});

#if DEBUG
		builder.Logging.AddDebug();

#endif

		builder.Services.AddSingleton<IConfiguracoesDasJuntasService, ConfiguracoesDasJuntasService>();
		builder.Services.AddSingleton<IHiveMqService, HiveMqService>();
		return builder.Build();
	}
}
