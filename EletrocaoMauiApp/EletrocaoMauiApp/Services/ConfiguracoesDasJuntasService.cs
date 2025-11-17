using SQLite;
using EletrocaoMauiApp.Models;

namespace EletrocaoMauiApp.Services;

public interface IConfiguracoesDasJuntasService
{
	Task<ConfiguracoesDaJunta> GetConfiguracoesDaJuntaAsync(int id);
	Task<ConfiguracoesDaJunta> GetConfiguracoesDaJuntaPorNomeAsync(string nome);
	Task<List<ConfiguracoesDaJunta>> GetAllConfiguracoesDasJuntasAsync();
	Task CreateConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta);
	Task DeleteConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta);
	Task UpdateConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta);

}
public class ConfiguracoesDasJuntasService : IConfiguracoesDasJuntasService
{
	private readonly SQLiteAsyncConnection _database;

	public ConfiguracoesDasJuntasService()
	{
		var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eletrocao.db");
		_database = new SQLiteAsyncConnection(dbPath);
		_database.CreateTableAsync<ConfiguracoesDaJunta>().Wait();
	}

	public async Task<ConfiguracoesDaJunta> GetConfiguracoesDaJuntaAsync(int id)
	{
		try
		{
			return await _database.Table<ConfiguracoesDaJunta>().Where(c => c.Id == id).FirstOrDefaultAsync();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task<ConfiguracoesDaJunta> GetConfiguracoesDaJuntaPorNomeAsync(string nome)
	{
		try
		{
			return await _database.Table<ConfiguracoesDaJunta>().Where(c => c.Nome == nome).FirstOrDefaultAsync();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task<List<ConfiguracoesDaJunta>> GetAllConfiguracoesDasJuntasAsync()
	{
		try
		{
			return await _database.Table<ConfiguracoesDaJunta>().ToListAsync();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task CreateConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta)
	{
		try
		{
			await _database.InsertAsync(configuracaoDaJunta);
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task DeleteConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta)
	{
		try
		{
			await _database.DeleteAsync(configuracaoDaJunta);
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task UpdateConfiguracoesDasJuntasAsync(ConfiguracoesDaJunta configuracaoDaJunta)
	{
		try
		{
			await _database.UpdateAsync(configuracaoDaJunta);
		}
		catch (Exception)
		{
			throw;
		}
	}
}
