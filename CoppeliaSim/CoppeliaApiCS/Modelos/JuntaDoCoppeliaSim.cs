namespace CoppeliaApiCS.Modelos;

internal record JuntaDoCoppeliaSim(
	string Nome,
	int Id,
	short AnguloMin,
	short AnguloMax,
	short AnguloInicial);
