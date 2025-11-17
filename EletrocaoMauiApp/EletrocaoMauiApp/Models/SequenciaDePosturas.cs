namespace EletrocaoMauiApp.Models;

public record SequenciaDePosturas(
	string Nome,
	ICollection<Postura> Posturas,
	int? Passos,
	int? Delay,
	int? Repeticoes);
