using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Models;

public record SequenciaDeComandosDasJuntasEsp32(
	string Nome,
	ICollection<ComandosJuntasEsp32> Comandos,
	int? Passos,
	int? Delay,
	int? Repeticoes,
	long Timestamp);