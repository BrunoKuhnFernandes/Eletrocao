namespace EletrocaoMauiApp.Models;
public record ComandosJuntasEsp32(
	ICollection<PwmParaJunta> PwmParaJuntas);