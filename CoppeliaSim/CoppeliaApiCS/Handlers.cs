using CoppeliaApiCS;
using CoppeliaApiCS.Modelos;
using System.Text.Json;

internal static class Handlers
{
	private static Coppelia _coppelia;
	private static IList<ConfiguracaoDaJunta> _parametrosJuntas;

	internal static void Initialize(Coppelia coppelia, IList<ConfiguracaoDaJunta> parametrosJuntas)
	{
		_coppelia = coppelia;
		_parametrosJuntas = parametrosJuntas;
	}

	internal static void HandleConfiguracoesJuntasCoppeliaSim(string message)
	{
		try
		{
			var configuracoesDaMensagem = JsonSerializer.Deserialize<List<ConfiguracaoDaJunta>>(message);
			if (!_parametrosJuntas.Any())
				_parametrosJuntas = configuracoesDaMensagem;
			else
				_coppelia.ConfigurarJuntasDoCoppeliaSim(configuracoesDaMensagem);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro no tópico 'parametros': {ex.Message}");
		}
	}

	internal static void HandleControleAngulos(string message)
	{
		try
		{
			IEnumerable<AnguloParaJunta> angulosParaJuntas =
				JsonSerializer.Deserialize<IEnumerable<AnguloParaJunta>>(message);

			if (angulosParaJuntas is not null)
				_coppelia.ComandarJuntasPorAngulo(angulosParaJuntas);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro: {ex.Message}");
		}
	}

	internal static async void HandleControleSequenciaDeComandos(string message)
	{
		try
		{
			SequenciaDeComandosDasJuntas? sequenciaDeComandos =
				JsonSerializer.Deserialize<SequenciaDeComandosDasJuntas>(message);

			if (sequenciaDeComandos is not null)
				await _coppelia.ComandarJuntasPorSequenciaDeComandosAsync(sequenciaDeComandos);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro: {ex.Message}");
		}
	}

	internal static void HandleInstrucoes(string message)
	{
		try
		{
			Instrucao? comando = JsonSerializer.Deserialize<Instrucao>(message);

			if (comando is not null)
				_coppelia.AtribuirInstrucaoAsync(comando);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro: {ex.Message}");
		}
	}
}
