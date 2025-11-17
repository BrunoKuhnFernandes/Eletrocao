using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Mappers;

public static class AnguloDaJuntaExtensions
{
	public static PwmParaJunta MapearAnguloParaPwm(this AnguloParaJunta anguloParaJunta, ConfiguracoesDaJunta configuracao)
	{
		float angulo = anguloParaJunta.Angulo;
		float pwm0 = configuracao.PwmMin;
		float pwm90 = configuracao.PwmCentro;
		float pwm180 = configuracao.PwmMax;
		int pino = configuracao.PwmPin;

		if (angulo < configuracao.AnguloLimiteMin || angulo > configuracao.AnguloLimiteMax)
			throw new ArgumentOutOfRangeException(nameof(angulo), "O ângulo está fora dos limites da configuração da junta.");

		if (angulo <= 90)
		{
			float pwm = pwm0 + (int)((pwm90 - pwm0) * (angulo / 90.0));
			return new PwmParaJunta(pino, pwm);
		}
		else
		{
			float pwm = pwm90 + (int)((pwm180 - pwm90) * ((angulo - 90) / 90.0));
			return new PwmParaJunta(pino, pwm);
		}
	}
}
