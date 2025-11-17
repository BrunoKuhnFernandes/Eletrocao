using EletrocaoMauiApp.Models;
using EletrocaoMauiApp.Services;

namespace EletrocaoMauiApp.Views;

public partial class ConfiguracaoDaJuntaPage : ContentPage
{
	private ConfiguracoesDaJunta _configuracao;
	private IConfiguracoesDasJuntasService _configuracoesJuntasService;
	private int _parametroId;

	public ConfiguracaoDaJuntaPage(int id, IConfiguracoesDasJuntasService servomotorService)
	{
		InitializeComponent();
		_parametroId = id;
		_configuracoesJuntasService = servomotorService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		_configuracao = await GetConfiguracoesDaJuntaAsync(_parametroId);
		AtualizarUI();
	}

	private async Task<ConfiguracoesDaJunta> GetConfiguracoesDaJuntaAsync(int servoId)
	{
		var servomotor = await _configuracoesJuntasService.GetConfiguracoesDaJuntaAsync(servoId);

		if (servomotor is null)
		{
			await DisplayAlert("Erro", "Não foi possível obter o servomotor.", "OK");

		}

		return servomotor;
	}

	private void AtualizarUI()
	{
		//LabelId.Text = _configuracao.Id.ToString();
		Nome_Entry.Text = _configuracao.Nome;
		LabelPwmPin.Text = _configuracao.PwmPin.ToString();

		LabelPwmMin.Text = _configuracao.PwmMin.ToString();
		SliderPwmMin.Value = _configuracao.PwmMin;

		LabelPwmCentro.Text = _configuracao.PwmCentro.ToString();
		SliderPwmCentro.Value = _configuracao.PwmCentro;

		LabelPwmMax.Text = _configuracao.PwmMax.ToString();
		SliderPwmMax.Value = _configuracao.PwmMax;

		LabelAnguloInicial.Text = _configuracao.AnguloInicial.ToString();
		SliderAnguloInicial.Value = _configuracao.AnguloInicial;

		LabelAnguloMin.Text = _configuracao.AnguloLimiteMin.ToString();
		SliderAnguloMin.Value = _configuracao.AnguloLimiteMin;

		LabelAnguloMax.Text = _configuracao.AnguloLimiteMax.ToString();
		SliderAnguloMax.Value = _configuracao.AnguloLimiteMax;

		RadioNormal.IsChecked = _configuracao.Invertido == true;
		RadioInvertido.IsChecked = _configuracao.Invertido == false;
	}

	private void OnIncreasePwmPin(object sender, EventArgs e)
	{
		_configuracao.PwmPin++;
		AtualizarUI();
	}

	private void OnDecreasePwmPin(object sender, EventArgs e)
	{
		if (_configuracao.PwmPin > 0) 
			_configuracao.PwmPin--;
		AtualizarUI();
	}

	// PWM Min
	private void OnIncreasePwmMin(object sender, EventArgs e)
	{
		if (_configuracao.PwmMin < 600) _configuracao.PwmMin++;
		SliderPwmMin.Value = _configuracao.PwmMin;
		AtualizarUI();
	}

	private void OnDecreasePwmMin(object sender, EventArgs e)
	{
		if (_configuracao.PwmMin > 0) _configuracao.PwmMin--;
		SliderPwmMin.Value = _configuracao.PwmMin;
		AtualizarUI();
	}

	private void OnSliderPwmMinChanged(object sender, ValueChangedEventArgs e)
	{
		_configuracao.PwmMin = (short)e.NewValue;
		AtualizarUI();
	}

	// PWM Centro
	private void OnIncreasePwmCentro(object sender, EventArgs e)
	{
		if (_configuracao.PwmCentro < 600) _configuracao.PwmCentro++;
		SliderPwmCentro.Value = _configuracao.PwmCentro;
		AtualizarUI();
	}

	private void OnDecreasePwmCentro(object sender, EventArgs e)
	{
		if (_configuracao.PwmCentro > 0) _configuracao.PwmCentro--;
		SliderPwmCentro.Value = _configuracao.PwmCentro;
		AtualizarUI();
	}

	private void OnSliderPwmCentroChanged(object sender, ValueChangedEventArgs e)
	{
		_configuracao.PwmCentro = (short)e.NewValue;
		AtualizarUI();
	}

	// PWM Max
	private void OnIncreasePwmMax(object sender, EventArgs e)
	{
		if (_configuracao.PwmMax < 600) _configuracao.PwmMax++;
		SliderPwmMax.Value = _configuracao.PwmMax;
		AtualizarUI();
	}

	private void OnDecreasePwmMax(object sender, EventArgs e)
	{
		if (_configuracao.PwmMax > 0) _configuracao.PwmMax--;
		SliderPwmMax.Value = _configuracao.PwmMax;
		AtualizarUI();
	}

	private void OnSliderPwmMaxChanged(object sender, ValueChangedEventArgs e)
	{
		_configuracao.PwmMax = (short)e.NewValue;
		AtualizarUI();
	}

	// Ângulo Inicial
	private void OnIncreaseAnguloInicial(object sender, EventArgs e)
	{
		if (_configuracao.AnguloInicial < 180) _configuracao.AnguloInicial++;
		SliderAnguloInicial.Value = _configuracao.AnguloInicial;
		AtualizarUI();
	}

	private void OnDecreaseAnguloInicial(object sender, EventArgs e)
	{
		if (_configuracao.AnguloInicial > 0) _configuracao.AnguloInicial--;
		SliderAnguloInicial.Value = _configuracao.AnguloInicial;
		AtualizarUI();
	}

	private void OnSliderAnguloInicialChanged(object sender, ValueChangedEventArgs e)
	{
		_configuracao.AnguloInicial = (short)e.NewValue;
		AtualizarUI();
	}

	// Ângulo Mín
	private void OnIncreaseAnguloMin(object sender, EventArgs e)
	{
		if (_configuracao.AnguloLimiteMin < 180) _configuracao.AnguloLimiteMin++;
		SliderAnguloMin.Value = _configuracao.AnguloLimiteMin;
		AtualizarUI();
	}

	private void OnDecreaseAnguloMin(object sender, EventArgs e)
	{
		if (_configuracao.AnguloLimiteMin > 0) _configuracao.AnguloLimiteMin--;
		SliderAnguloMin.Value = _configuracao.AnguloLimiteMin;
		AtualizarUI();
	}

	private void OnSliderAnguloMinChanged(object sender, ValueChangedEventArgs e)
	{                 
		_configuracao.AnguloLimiteMin = (short)e.NewValue;
		AtualizarUI();
	}

	// Ângulo Máx
	private void OnIncreaseAnguloMax(object sender, EventArgs e)
	{
		if (_configuracao.AnguloLimiteMax < 180) _configuracao.AnguloLimiteMax++;
		SliderAnguloMax.Value = _configuracao.AnguloLimiteMax;
		AtualizarUI();
	}

	private void OnDecreaseAnguloMax(object sender, EventArgs e)
	{
		if (_configuracao.AnguloLimiteMax > 0) _configuracao.AnguloLimiteMax--;
		SliderAnguloMax.Value = _configuracao.AnguloLimiteMax;
		AtualizarUI();
	}

	private void OnSliderAnguloMaxChanged(object sender, ValueChangedEventArgs e)
	{
		_configuracao.AnguloLimiteMax = (short)e.NewValue;
		AtualizarUI();
	}

	private void OnDirecaoChanged(object sender, CheckedChangedEventArgs e)
	{
		if (RadioNormal.IsChecked)
			_configuracao.Invertido = true;
		else if (RadioInvertido.IsChecked)
			_configuracao.Invertido = false;
	}

	private async void OnSalvar(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(Nome_Entry.Text)) 
		{
			await DisplayAlert("Erro", "O nome da junta não pode ser vazio.", "OK");
			return;
		}
		_configuracao.Nome = Nome_Entry.Text;

		string servoConfig = /*$"Configuração para a junta de Id: {LabelId.Text}\n" +*/
							 $"Pwm min: {LabelPwmMin.Text}\n" +
							 $"Pwm centro: {LabelPwmCentro.Text}\n" +
							 $"Pwm Max: {LabelPwmMax.Text}\n" +
							 $"Angulo Inicial: {LabelAnguloInicial.Text}\n" +
							 $"Angulo Min: {LabelAnguloMin.Text}\n" +
							 $"Angulo Max: {LabelAnguloMax.Text}\n" +
							 $"Direção: {_configuracao.Invertido}";
		await DisplayAlert("Salvo", $"Os dados de {_configuracao.Nome} foram armazenados.\n{servoConfig}", "OK");

		await _configuracoesJuntasService.UpdateConfiguracoesDasJuntasAsync(_configuracao);
	}

	private async void NavegarParaJuntas(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private async void OnRemover(object sender, EventArgs e)
	{
		
		await _configuracoesJuntasService.DeleteConfiguracoesDasJuntasAsync(_configuracao);
		await Navigation.PopAsync();
	}

	private void Nome_Entry_Unfocused(object sender, FocusEventArgs e)
	{
		_configuracao.Nome = Nome_Entry.Text;
	}
}
