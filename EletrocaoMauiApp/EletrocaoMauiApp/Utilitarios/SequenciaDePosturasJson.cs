using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EletrocaoMauiApp.Utilitarios;

internal class SequenciaDePosturasJson
{
	internal async Task CopiarArquivosDeRawParaAppDataAsync()
	{
		string[] arquivos = new[]
		{
		"caminhar.json",
		"caminharTras.json",
		"deitar.json",
		"girarDireita.json",
		"girarEsquerda.json",
		"levantar.json",
		"levantarPata.json",
		"sentar.json"
	};

		foreach (var arquivo in arquivos)
		{
			var destino = Path.Combine(FileSystem.AppDataDirectory, arquivo);

			// Só copia se ainda não existir
			if (File.Exists(destino))
				continue;

			using var stream = await FileSystem.OpenAppPackageFileAsync(arquivo);
			using var reader = new StreamReader(stream);
			var conteudo = await reader.ReadToEndAsync();

			await File.WriteAllTextAsync(destino, conteudo);
		}
	}
}
