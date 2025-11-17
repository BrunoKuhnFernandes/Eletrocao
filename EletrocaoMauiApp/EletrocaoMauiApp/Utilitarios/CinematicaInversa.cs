using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Utilitarios;
internal class CinematicaInversa
{
	private const double A = 60.5;
	private const double B = 10.0;  
	private const double E = 111.126;
	private const double F = 118.5;   

	internal (float, float, float) CalcularAngulosDaPerna(float x, float y, float z)
	{
		double C = Math.Sqrt(y * (double)y + z * (double)z);
		double H = Math.Sqrt(Math.Max(0.0, C * C - A * A));
		double D = H - B;
		double G = Math.Sqrt((double)x * x + D * D);

		double omegaRad = Math.Atan2(z, y) + Math.Atan2(H, A);
		float omega = (float)(omegaRad * 180.0 / Math.PI);
		omega = 180 - omega;

		double cosPhi = (G * G - E * E - F * F) / (-2.0 * E * F);
		cosPhi = Math.Max(-1.0, Math.Min(1.0, cosPhi));
		double phiRad = Math.Acos(cosPhi);
		float phi = (float)(phiRad * 180.0 / Math.PI);

		double epsilonRad = Math.Atan2(x, D);
		double sinBeta = F * Math.Sin(phiRad) / G;
		sinBeta = Math.Max(-1.0, Math.Min(1.0, sinBeta));
		double betaRad = Math.Asin(sinBeta);
		
		double thetaRad = epsilonRad + betaRad;
		float theta = (float)(A + thetaRad * 180.0 / Math.PI);
		
		return (omega, theta, phi);
	}
}