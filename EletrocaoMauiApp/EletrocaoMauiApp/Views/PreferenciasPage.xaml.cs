using EletrocaoMauiApp.Services;
using System.Globalization;

namespace EletrocaoMauiApp.Views;

public partial class PreferenciasPage : ContentPage
{
	private string _currentLanguage = "pt";

	public PreferenciasPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		_currentLanguage = Preferences.Get("Culture", "pt");
		SetRadioButtons();
		hostEntry.Text = Preferences.Get("Host", "");
		portEntry.Text = Preferences.Get("Port", 0000).ToString();
	}

	private async void Salvar_Btn_Clicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(hostEntry.Text) || string.IsNullOrWhiteSpace(portEntry.Text))
		{
			await DisplayAlert("Erro", "Host e Port não podem estar vazios.", "OK");
			return;
		}

		if (!int.TryParse(portEntry.Text, out int port))
		{
			await DisplayAlert("Erro", "Porta deve conter apenas números.", "OK");
			return;
		}

		Preferences.Set("Host", hostEntry.Text);
		Preferences.Set("Port", port);

		await DisplayAlert("Sucesso", "Configurações salvas.", "OK");
	}
	private async void Voltar_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}


	private void Radio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_currentLanguage = (sender as RadioButton)?.Value?.ToString() ?? "en";

		SetLanguage(_currentLanguage);
	}

	private void SetRadioButtons()
	{
		if (_currentLanguage == "pt")
			RadioBR.IsChecked = true;
		else
			RadioEN.IsChecked = true;
	}

	private void SetLanguage(string language)
	{
		
		_currentLanguage = language;
		App.SetCulture(language);
	}

	private void Bandeira_Clicked(object sender, EventArgs e)
	{
		_currentLanguage = (sender as Button)?.StyleId == "BRButton" ? "pt" : "en";

		SetRadioButtons();

		SetLanguage(_currentLanguage);
	}
}