using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Utilitarios;

namespace EletrocaoMauiApp.Mappers;
public static class SequenciaDePosturasExtensions
{
	public static (SequenciaDeComandosDasJuntas, SequenciaDeComandosDasJuntasEsp32) MapearParaSequenciaDeComandos(
		this SequenciaDePosturas sequenciaDePosturas,
		IEnumerable<ConfiguracoesDaJunta> configuracoesDasJuntas, long timeStamp)
	{
		if (sequenciaDePosturas == null)
			throw new ArgumentNullException(nameof(sequenciaDePosturas));

		CinematicaInversa c = new();

		IList<ComandosJuntas> comandos = [];
		IList<ComandosJuntasEsp32> comandosEsp32 = [];

		foreach (Postura postura in sequenciaDePosturas.Posturas)
		{
			List<AnguloParaJunta> angulosParaAsJuntas = [];
			List<AnguloParaJunta> angulosParaAsJuntasAjustados = [];
			List<PwmParaJunta> pwmParaJuntas = [];
			foreach (var coordenada in postura.CoordenadasDosMembros)
			{
				(float omega, float theta, float phi) = c.CalcularAngulosDaPerna(coordenada.X, coordenada.Y, coordenada.Z);
				angulosParaAsJuntas.Add(new AnguloParaJunta($"{coordenada.SiglaDoMembro}_ombro_junta", omega));
				angulosParaAsJuntas.Add(new AnguloParaJunta($"{coordenada.SiglaDoMembro}_perna_superior_junta", theta));
				angulosParaAsJuntas.Add(new AnguloParaJunta($"{coordenada.SiglaDoMembro}_perna_inferior_junta", phi));
			}
			foreach (var anguloParaJunta in angulosParaAsJuntas)
			{
				var config = configuracoesDasJuntas.FirstOrDefault(c => anguloParaJunta.Nome == c.Nome);
				if (config == null)
					throw new Exception();

				bool invertido = config.Invertido;
				AnguloParaJunta angulo = invertido ? anguloParaJunta : new(anguloParaJunta.Nome, 180 - anguloParaJunta.Angulo);

				pwmParaJuntas.Add(angulo.MapearAnguloParaPwm(config));
				angulosParaAsJuntasAjustados.Add(angulo);
			}
			
			comandos.Add(new(angulosParaAsJuntasAjustados));
			comandosEsp32.Add(new(pwmParaJuntas.OrdenarPorPino()));
		}



		return (
			new SequenciaDeComandosDasJuntas(
				sequenciaDePosturas.Nome,
				comandos,
				sequenciaDePosturas.Passos,
				sequenciaDePosturas.Delay,
				sequenciaDePosturas.Repeticoes, timeStamp),
			new SequenciaDeComandosDasJuntasEsp32(
				sequenciaDePosturas.Nome,
				comandosEsp32,
				sequenciaDePosturas.Passos,
				sequenciaDePosturas.Delay,
				sequenciaDePosturas.Repeticoes, timeStamp));
	}
}