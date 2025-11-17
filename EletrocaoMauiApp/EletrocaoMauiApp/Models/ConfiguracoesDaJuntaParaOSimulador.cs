namespace EletrocaoMauiApp.Models;

public class ConfiguracoesDaJuntaParaOSimulador
{
	public string Nome { get; set; } = string.Empty;
	public short AnguloLimiteMax { get; set; }
	public short AnguloLimiteMin { get; set; }
	public short AnguloInicial { get; set; }
}
