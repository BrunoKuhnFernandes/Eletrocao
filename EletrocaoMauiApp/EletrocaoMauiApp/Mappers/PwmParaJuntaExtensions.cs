using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Mappers;

public static class PwmParaJuntaExtensions
{
	public static List<PwmParaJunta> OrdenarPorPino(this List<PwmParaJunta>  pwmParaJuntas)
	{
		return pwmParaJuntas
			.OrderBy(p => p.Pino)
			.ToList();
	}
}
