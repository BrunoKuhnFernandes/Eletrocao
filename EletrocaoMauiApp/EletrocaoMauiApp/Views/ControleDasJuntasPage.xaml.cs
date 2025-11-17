using EletrocaoMauiApp.Mappers;
using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;
using EletrocaoMauiApp.Utilitarios;
using System.Text.Json;

namespace EletrocaoMauiApp.Views;

public partial class ControleDasJuntasPage : ContentPage
{
	private readonly IHiveMqService _hiveMqService;
	private readonly IConfiguracoesDasJuntasService _configuracoesJuntasService;
	private IEnumerable<ConfiguracoesDaJunta> _configuracoesDasJuntas = [];
	private string _siglaDaPernaSelecionada = string.Empty;
	private ConfiguracoesDaJunta _configuaracoesJunta1 = new();
	private ConfiguracoesDaJunta _configuaracoesJunta2 = new();
	private ConfiguracoesDaJunta _configuaracoesJunta3 = new();

	public ControleDasJuntasPage(IHiveMqService hiveMqService, IConfiguracoesDasJuntasService configuracoesDasJuntasService)
	{
		InitializeComponent();
		_hiveMqService = hiveMqService;
		_configuracoesJuntasService = configuracoesDasJuntasService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		_configuracoesDasJuntas = await GetConfiguracoesDasJuntasAsync();
	}

	private async Task<IEnumerable<ConfiguracoesDaJunta>> GetConfiguracoesDasJuntasAsync()
	{
		var configuracoesDasJuntas = await _configuracoesJuntasService.GetAllConfiguracoesDasJuntasAsync();
		if (configuracoesDasJuntas is null)
		{
			await DisplayAlert("Erro", "Não foi possível obter as juntas.", "OK");
			return [];
		}
		return configuracoesDasJuntas;
	}

	private void SelecionarPerna(object sender, EventArgs e)
	{
		var opcaoSelecionada = ((Picker)sender).SelectedItem?.ToString();

		_siglaDaPernaSelecionada = opcaoSelecionada switch
		{
			"Perna esquerda frontal" => "EF",
			"Perna direita frontal" => "DF",
			"Perna esquerda traseira" => "ET",
			"Perna direita traseira" => "DT",
			_ => string.Empty
		};

		FrameControle1.IsVisible = true;
		FrameControle2.IsVisible = true;
		FrameControle3.IsVisible = true;
		FrameRadioBtns.IsVisible = true;
		ControlarBtn.IsVisible = true;

		CapturarConfiguracoesDasJuntasDaPerna(_siglaDaPernaSelecionada);
		AtualizarSlidersELabels();
	}

	private async void CapturarConfiguracoesDasJuntasDaPerna(string siglaPerna)
	{
		_configuaracoesJunta1 = await _configuracoesJuntasService.GetConfiguracoesDaJuntaPorNomeAsync(siglaPerna + "_ombro_junta");
		_configuaracoesJunta2 = await _configuracoesJuntasService.GetConfiguracoesDaJuntaPorNomeAsync(siglaPerna + "_perna_superior_junta");
		_configuaracoesJunta3 = await _configuracoesJuntasService.GetConfiguracoesDaJuntaPorNomeAsync(siglaPerna + "_perna_inferior_junta");
		AtualizarSlidersELabels();
	}

	private async void NavegarParaJuntas(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void AlterarValorControle(Slider slider, Label label, bool aumentar)
	{
		double valorAtual = slider.Value;
		valorAtual += aumentar ? 1 : -1;
		valorAtual = Math.Clamp(valorAtual, slider.Minimum, slider.Maximum);
		slider.Value = valorAtual;
		label.Text = valorAtual.ToString("0");
	}

	private void AumentarControle1(object sender, EventArgs e) => AlterarValorControle(SliderControle1, LabelControle1, true);
	private void DiminuirControle1(object sender, EventArgs e) => AlterarValorControle(SliderControle1, LabelControle1, false);
	private void AumentarControle2(object sender, EventArgs e) => AlterarValorControle(SliderControle2, LabelControle2, true);
	private void DiminuirControle2(object sender, EventArgs e) => AlterarValorControle(SliderControle2, LabelControle2, false);
	private void AumentarControle3(object sender, EventArgs e) => AlterarValorControle(SliderControle3, LabelControle3, true);
	private void DiminuirControle3(object sender, EventArgs e) => AlterarValorControle(SliderControle3, LabelControle3, false);

	private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (sender == SliderControle1) LabelControle1.Text = e.NewValue.ToString("0");
		else if (sender == SliderControle2) LabelControle2.Text = e.NewValue.ToString("0");
		else if (sender == SliderControle3) LabelControle3.Text = e.NewValue.ToString("0");
	}

	private void AtualizarSlidersELabels()
	{
		void Configurar(Slider s, Label l, double min, double max)
		{
			s.Minimum = min;
			s.Maximum = max;
			s.Value = (max - min) / 2 + min;
			l.Text = s.Value.ToString("0");
		}

		if (RadioAngulo.IsChecked)
		{
			Label1.Text = "Ângulo do ombro";
			Label2.Text = "Ângulo da perna superior";
			Label3.Text = "Ângulo da perna inferior";

			Configurar(SliderControle1, LabelControle1, _configuaracoesJunta1.AnguloLimiteMin, _configuaracoesJunta1.AnguloLimiteMax);
			Configurar(SliderControle2, LabelControle2, _configuaracoesJunta2.AnguloLimiteMin, _configuaracoesJunta2.AnguloLimiteMax);
			Configurar(SliderControle3, LabelControle3, _configuaracoesJunta3.AnguloLimiteMin, _configuaracoesJunta3.AnguloLimiteMax);
		}
		else if (RadioPwm.IsChecked)
		{
			Label1.Text = "PWM do ombro";
			Label2.Text = "PWM da perna superior";
			Label3.Text = "PWM da perna inferior";

			Configurar(SliderControle1, LabelControle1, _configuaracoesJunta1.PwmMin, _configuaracoesJunta1.PwmMax);
			Configurar(SliderControle2, LabelControle2, _configuaracoesJunta2.PwmMin, _configuaracoesJunta2.PwmMax);
			Configurar(SliderControle3, LabelControle3, _configuaracoesJunta3.PwmMin, _configuaracoesJunta3.PwmMax);
		}
		else if (RadioCoordenadas.IsChecked)
		{
			Label1.Text = "Coordenada X";
			Label2.Text = "Coordenada Y";
			Label3.Text = "Coordenada Z";

			Configurar(SliderControle1, LabelControle1, 0, 100);
			Configurar(SliderControle2, LabelControle2, 0, 300);
			Configurar(SliderControle3, LabelControle3, 0, 120);
		}
	}

	private void ControlarRadioChanged(object sender, CheckedChangedEventArgs e)
	{
		AtualizarSlidersELabels();
	}

	private async void ControlarJunta(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(_siglaDaPernaSelecionada))
		{
			await DisplayAlert("Erro", "Selecione uma perna para controlar.", "OK");
			return;
		}

		string comando = string.Empty;
		string comandoEsp32 = string.Empty;

		if (RadioAngulo.IsChecked)
		{
			IEnumerable<AnguloParaJunta> angulosParajuntas = [
				new($"{_siglaDaPernaSelecionada}_ombro_junta", (float)SliderControle1.Value),
				new($"{_siglaDaPernaSelecionada}_perna_superior_junta", (float)SliderControle2.Value),
				new($"{_siglaDaPernaSelecionada}_perna_inferior_junta", (float)SliderControle3.Value)
			];

			IEnumerable<PwmParaJunta> pwmParaJuntas =
				angulosParajuntas.Select(a => a.MapearAnguloParaPwm(_configuracoesDasJuntas.First(c => c.Nome == a.Nome)));

			comando = JsonSerializer.Serialize(angulosParajuntas);
			comandoEsp32 = JsonSerializer.Serialize(pwmParaJuntas);
			_hiveMqService.Publish(comandoEsp32, Constantes.Topicos.TopicoParaControlePorPwmEsp32);
			_hiveMqService.Publish(comando, Constantes.Topicos.TopicoParaControlePorAngulo);
		}
		else if (RadioPwm.IsChecked)
		{
			comandoEsp32 = JsonSerializer.Serialize<IEnumerable<PwmParaJunta>>([
				new(_configuaracoesJunta1.PwmPin, (float)SliderControle1.Value),
				new(_configuaracoesJunta2.PwmPin, (float)SliderControle2.Value),
				new(_configuaracoesJunta3.PwmPin, (float)SliderControle3.Value)
			]);
			_hiveMqService.Publish(comandoEsp32, Constantes.Topicos.TopicoParaControlePorPwmEsp32);
		}
		else if (RadioCoordenadas.IsChecked)
		{
			
			List<AnguloParaJunta> angulosParaAsJuntasAjustados = [];
			var coordenadaX = (float)SliderControle1.Value;
			var coordenadaY = (float)SliderControle2.Value;
			var coordenadaZ = (float)SliderControle3.Value;
			CinematicaInversa c = new();

			(var omega, var theta, var phi) = c.CalcularAngulosDaPerna(coordenadaX, coordenadaY, coordenadaZ);

			void AjustarJunta(ConfiguracoesDaJunta conf, string nomeBase, float angulo)
			{
				if (!conf.Invertido)
					angulo = 180 - angulo;
				angulosParaAsJuntasAjustados.Add(new AnguloParaJunta(nomeBase, angulo));
			}

			AjustarJunta(_configuaracoesJunta1, $"{_siglaDaPernaSelecionada}_ombro_junta", omega);
			AjustarJunta(_configuaracoesJunta2, $"{_siglaDaPernaSelecionada}_perna_superior_junta", theta);
			AjustarJunta(_configuaracoesJunta3, $"{_siglaDaPernaSelecionada}_perna_inferior_junta", phi);

			IEnumerable<PwmParaJunta> pwmParaJuntas =
				angulosParaAsJuntasAjustados.Select(a => a.MapearAnguloParaPwm(_configuracoesDasJuntas.First(c => c.Nome == a.Nome)));

			comandoEsp32 = JsonSerializer.Serialize(pwmParaJuntas);
			comando = JsonSerializer.Serialize(angulosParaAsJuntasAjustados);

			_hiveMqService.Publish(comando, Constantes.Topicos.TopicoParaControlePorAngulo);
			_hiveMqService.Publish(comandoEsp32, Constantes.Topicos.TopicoParaControlePorPwmEsp32);
		}
		else
		{
			await DisplayAlert("Erro", "Selecione um modo de controle.", "OK");
			return;
		}
	}
}
