namespace EletrocaoMauiApp.Models;

public class InformacoesDoRobo
{
	public double TensaoBateriaServomotores { get; set; }
	public double TensaoBateriaMicro { get; set; }
	public double Corrente { get; set; }
	public string Estado { get; set; } = string.Empty;
	public string Mensagem { get; set; } = string.Empty;
	public TimeSpan Timestamp { get; set; }
	public int ContadorDeMensagens { get; set; }
}