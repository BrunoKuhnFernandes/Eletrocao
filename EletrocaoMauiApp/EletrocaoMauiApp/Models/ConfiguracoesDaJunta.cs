using SQLite;

namespace EletrocaoMauiApp.Models;

public class ConfiguracoesDaJunta
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string Nome { get; set; } = string.Empty;
	public int PwmPin { get; set; }
	public short PwmMin { get; set; }
	public short PwmCentro { get; set; }
	public short PwmMax { get; set; }
	public short AnguloLimiteMax { get; set; }
	public short AnguloInicial{ get; set; }
	public short AnguloLimiteMin { get; set; }
	public bool Invertido { get; set; }
}
