using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Models;

public record SequenciaDeComandosDasJuntas(
	string Nome,
	ICollection<ComandosJuntas> ComandosJuntas,
	int? Passos,
	int? Delay,
	int? Repeticoes);