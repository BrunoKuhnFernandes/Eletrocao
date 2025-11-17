using EletrocaoMauiApp.Models;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class SequenciaDePosturasPage : ContentPage
{
	private List<SequenciaDePosturas> _sequencias = new();
	private SequenciaDePosturas _sequenciaSelecionada;

	public SequenciaDePosturasPage()
	{
		InitializeComponent();
		CarregarSequencias();
	}

	private void CarregarSequencias()
	{
		// Carregando todos os jsons, de maneira hardcooded ainda
		var jsonCaminhar = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "caminhar.json"));
		SequenciaDePosturas sequenciaCaminhar = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonCaminhar) ?? throw new Exception("Não foi possível carregar a sequencia caminhar");
		var jsonCaminharTras = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "caminharTras.json"));
		SequenciaDePosturas sequenciaCaminharTras = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonCaminharTras) ?? throw new Exception("Não foi possível carregar a sequencia caminharTras");
		var jsonDeitar = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "deitar.json"));
		SequenciaDePosturas sequenciaDeitar = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonDeitar) ?? throw new Exception("Não foi possível carregar a sequencia deitar");
		var jsonGirarDireita = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "girarDireita.json"));
		SequenciaDePosturas sequenciaGirarDireita = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonGirarDireita) ?? throw new Exception("Não foi possível carregar a sequencia girarDireita");
		var jsonGirarEsquerda = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "girarEsquerda.json"));
		SequenciaDePosturas sequenciaGirarEsquerda = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonGirarEsquerda) ?? throw new Exception("Não foi possível carregar a sequencia girarEsquerda");
		var jsonLevantar = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "levantar.json"));
		SequenciaDePosturas sequenciaLevantar = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonLevantar) ?? throw new Exception("Não foi possível carregar a sequencia levantar");
		var jsonLevantarPata = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "levantarPata.json"));
		SequenciaDePosturas sequenciaLevantarPata = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonLevantarPata) ?? throw new Exception("Não foi possível carregar a sequencia levantarPata");
		var jsonSentar = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "sentar.json"));
		SequenciaDePosturas sequenciaSentar = JsonSerializer.Deserialize<SequenciaDePosturas>(jsonSentar) ?? throw new Exception("Não foi possível carregar a sequencia sentar");

		_sequencias = [sequenciaCaminhar, sequenciaCaminharTras, sequenciaDeitar, sequenciaGirarDireita, sequenciaGirarEsquerda, sequenciaLevantar, sequenciaLevantarPata, sequenciaSentar];
		PickerSequencia.ItemsSource = _sequencias.Select(s => s.Nome).ToList();
	}

	private void OnSequenciaSelecionada(object sender, EventArgs e)
	{
		if (PickerSequencia.SelectedIndex < 0) return;

		_sequenciaSelecionada = _sequencias[PickerSequencia.SelectedIndex];
		BindingContext = _sequenciaSelecionada;
		PosturasCollection.ItemsSource = _sequenciaSelecionada.Posturas;
	}

	private void OnAdicionarPostura_Clicked(object sender, EventArgs e)
	{
		if (_sequenciaSelecionada == null) return;

		var novaPostura = new Postura(
			new List<CoordenadasDoMembro>(){
				new CoordenadasDoMembro ( "EF", 0,0,0),
				new CoordenadasDoMembro ( "DF", 0,0,0),
				new CoordenadasDoMembro ( "ET", 0,0,0),
				new CoordenadasDoMembro ( "DT", 0,0,0)
			});

		_sequenciaSelecionada.Posturas.Add(novaPostura);
		AtualizarLista();
	}

	private void OnRemoverPostura_Clicked(object sender, EventArgs e)
	{
		var postura = (sender as Button)?.CommandParameter as Postura;
		if (postura == null || _sequenciaSelecionada == null) return;

		_sequenciaSelecionada.Posturas.Remove(postura);
		AtualizarLista();
	}

	private void AtualizarLista()
	{

		PosturasCollection.ItemsSource = null;
		PosturasCollection.ItemsSource = _sequenciaSelecionada.Posturas;
	}

	private async void OnSalvarSequencia_Clicked(object sender, EventArgs e)
	{
		var json = JsonSerializer.Serialize(_sequenciaSelecionada, new JsonSerializerOptions { WriteIndented = true });
		var path = Path.Combine(FileSystem.AppDataDirectory, _sequenciaSelecionada.Nome + ".json");
		await File.WriteAllTextAsync(path, json);

		await DisplayAlert("Salvo", $"Sequência '{_sequenciaSelecionada.Nome}' atualizada!", "OK");
	}

	private async void Voltar_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}
