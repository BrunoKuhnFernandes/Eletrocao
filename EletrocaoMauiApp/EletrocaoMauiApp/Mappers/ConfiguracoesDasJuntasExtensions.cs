using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Mappers;
public static class ConfiguracoesDaJuntaExtensions
{
	public static ConfiguracoesDaJuntaParaOSimulador MapearParaSimulador(
		this ConfiguracoesDaJunta configuracaoDaJuntaDb)
	{
		if (configuracaoDaJuntaDb == null)
			throw new ArgumentNullException(nameof(configuracaoDaJuntaDb));

		return new ConfiguracoesDaJuntaParaOSimulador
		{
			Nome = configuracaoDaJuntaDb.Nome,
			AnguloLimiteMax = configuracaoDaJuntaDb.AnguloLimiteMax,
			AnguloLimiteMin = configuracaoDaJuntaDb.AnguloLimiteMin,
			AnguloInicial = configuracaoDaJuntaDb.AnguloInicial
		};
	}

	public static IEnumerable<ConfiguracoesDaJuntaParaOSimulador> MapearParaSimulador(
		this IEnumerable<ConfiguracoesDaJunta> configuracoesDasJuntasDB)
	{
		return configuracoesDasJuntasDB.Select(e => e.MapearParaSimulador());
	}
}