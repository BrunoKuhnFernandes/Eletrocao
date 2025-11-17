using CoppeliaApiCS;
using CoppeliaApiCS.Modelos;
using CoppeliaApiCS.Services;
using System.Text.Json;

Coppelia coppelia = new();
var mq = new HiveMqService("EletrocaoUser", "Baunilha1");
bool conectado = false;
IList<ConfiguracaoDaJunta> parametrosJuntas = [];

conectado = await mq.ConnectClientAsync();
while (!conectado)
{
	Console.WriteLine("Tentando conectar ao HiveMQ...");
	await Task.Delay(1000);
	conectado = await mq.ConnectClientAsync();
}
Console.WriteLine("Conectado ao HiveMQ!");

var topicHandlers = new Dictionary<string, Action<string>>
{
	{ Constantes.Topicos.TopicoParaConfigurarAsJuntasDoCoppeliaSim, HandleConfiguracoesJuntasCoppeliaSim }
};

mq.PrepareClientToSubscribe();
mq.SubscribeAsync(Constantes.Topicos.TopicoParaConfigurarAsJuntasDoCoppeliaSim);

mq.MessageReceived += (topic, message) =>
{
	if (topicHandlers.TryGetValue(topic, out var handler))
	{
		handler(message); // Chama o handler correto
	}
	else
	{
		Console.WriteLine($"Nenhum handler para o tópico: {topic}");
	}
};

while (!parametrosJuntas.Any())
{
	Console.WriteLine("Esperando a configuração dos joints...");
	await Task.Delay(2000);
}

conectado = false;
while (!conectado)
{
	Console.WriteLine("Tentando conectar ao CoppeliaSim...");
	await Task.Delay(1000);
	conectado = coppelia.ConectarAoCoppeliaSim();
}
Console.WriteLine("Conectado ao CoppeliaSim!");
coppelia.ConfigurarJuntasDoCoppeliaSim(parametrosJuntas);

topicHandlers.Add(Constantes.Topicos.TopicoParaControlePorSequenciaDeComandos, HandleControleSequenciaDeComandos);
mq.SubscribeAsync(Constantes.Topicos.TopicoParaControlePorSequenciaDeComandos);

topicHandlers.Add(Constantes.Topicos.TopicoParaControlePorAngulo, HandleControleAngulos);
mq.SubscribeAsync(Constantes.Topicos.TopicoParaControlePorAngulo);

topicHandlers.Add(Constantes.Topicos.TopicoParaInstrucoes, HandleInstrucoes);
mq.SubscribeAsync(Constantes.Topicos.TopicoParaInstrucoes);

void HandleConfiguracoesJuntasCoppeliaSim(string message)
{
	try
	{
		var configuracoesDaMensagem = JsonSerializer.Deserialize<List<ConfiguracaoDaJunta>>(message);
		if (!parametrosJuntas.Any())
			parametrosJuntas = configuracoesDaMensagem;
		else
			coppelia.ConfigurarJuntasDoCoppeliaSim(configuracoesDaMensagem);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Erro no tópico 'parametros': {ex.Message}");
	}
}

void HandleControleAngulos(string message)
{
	try
	{
		IEnumerable<AnguloParaJunta> angulosParaJuntas =
			JsonSerializer.Deserialize<IEnumerable<AnguloParaJunta>>(message);

		if (angulosParaJuntas is not null)
			coppelia.ComandarJuntasPorAngulo(angulosParaJuntas);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Erro: {ex.Message}");
	}
}

async void HandleControleSequenciaDeComandos(string message)
{
	try
	{
		SequenciaDeComandosDasJuntas? sequenciaDeComandos =
			JsonSerializer.Deserialize<SequenciaDeComandosDasJuntas>(message);

		if (sequenciaDeComandos is not null)
			await coppelia.ComandarJuntasPorSequenciaDeComandosAsync(sequenciaDeComandos);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Erro: {ex.Message}");
	}
}

void HandleInstrucoes(string message)
{
	try
	{
		Instrucao? comando = JsonSerializer.Deserialize<Instrucao>(message);

		if (comando is not null)
			coppelia.AtribuirInstrucaoAsync(comando);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Erro: {ex.Message}");
	}
}
