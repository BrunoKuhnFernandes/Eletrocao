using System;
using Microsoft.Maui.Controls;

namespace EletrocaoMauiApp.Converters
{
	public class NomeParaImagemConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string nome)
			{
				nome = nome.ToLowerInvariant();
				if (nome.Contains("ombro"))
					return "ombro.png";
				else if (nome.Contains("superior"))
					return "pernasuperior.png";
				else if (nome.Contains("inferior"))
					return "pernainferior.png";
			}

			return "default.png"; // caso não encontre nenhuma correspondência
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
