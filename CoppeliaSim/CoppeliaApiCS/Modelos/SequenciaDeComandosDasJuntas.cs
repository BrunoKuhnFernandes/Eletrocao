namespace CoppeliaApiCS.Modelos;

public record SequenciaDeComandosDasJuntas(
	string Nome,
	ICollection<ComandosJuntas> ComandosJuntas,
	int? Passos,
	int? Delay,
	int? Repeticoes);