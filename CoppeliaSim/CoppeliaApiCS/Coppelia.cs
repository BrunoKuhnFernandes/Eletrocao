using CoppeliaApiCS.Modelos;

namespace CoppeliaApiCS;
internal class Coppelia
{
	private int _coppelia;
	private ICollection<JuntaDoCoppeliaSim> _juntasDoCoppeliaSim = [];
	private readonly Dictionary<string, float> _ultimoAnguloPorJoint = new();
	private Instrucao _instrucao = new();

	public bool ConectarAoCoppeliaSim()
	{
		_coppelia = CoppeliaApi.Connect("127.0.0.1", 3000);
		return CoppeliaApi.IsConnected(_coppelia);
	}

	public void ConfigurarJuntasDoCoppeliaSim(IList<ConfiguracaoDaJunta> configuracoes)
	{
		if (!_juntasDoCoppeliaSim.Any())
		{
			foreach (ConfiguracaoDaJunta configuracao in configuracoes)
			{
				var idDoHandler = CoppeliaApi.GetObjectHandle(_coppelia, configuracao.Nome);
				if (idDoHandler < 0) throw new Exception($"Erro ao tentar obter o id da junta: {configuracao.Nome}");

				_juntasDoCoppeliaSim.Add(new JuntaDoCoppeliaSim(configuracao.Nome, idDoHandler, configuracao.AnguloLimiteMin, configuracao.AnguloLimiteMax, configuracao.AnguloInicial));
			}
		}
		else
		{
			IList<JuntaDoCoppeliaSim> juntas = [];
			foreach (var junta in _juntasDoCoppeliaSim)
			{
				var configuracao = configuracoes.FirstOrDefault(c => c.Nome == junta.Nome);
				if (configuracao != null)
					juntas.Add(new JuntaDoCoppeliaSim(configuracao.Nome, junta.Id, configuracao.AnguloLimiteMin, configuracao.AnguloLimiteMax, configuracao.AnguloInicial));
			}
			_juntasDoCoppeliaSim = juntas;
		}
	}

	public void DesconectarDoCoppeliaSim()
	{
		CoppeliaApi.Disconnect(_coppelia);
	}

	public void AtribuirInstrucaoAsync(Instrucao instrucao)
	{
		_instrucao.Nome = instrucao.Nome;
		_instrucao.Data = DateTime.Now;
	}

	private bool EstaConectado()
	{
		return CoppeliaApi.IsConnected(_coppelia);
	}

	private bool VericarSeOAnguloEstaDentroDoLimite(string nome, float angulo)
	{
		var junta = _juntasDoCoppeliaSim.FirstOrDefault(j => j.Nome == nome);
		if (junta is null)
			return false;

		return angulo >= junta.AnguloMin && angulo <= junta.AnguloMax;
	}

	public void ComandarJuntasPorAngulo(IEnumerable<AnguloParaJunta> angulosParaJuntas)
	{
		List<int> idsJointHandles = [];
		List<float> angulos = [];
		foreach (var anguloParaJunta in angulosParaJuntas)
		{
			if (!VericarSeOAnguloEstaDentroDoLimite(anguloParaJunta.Nome, anguloParaJunta.Angulo))
				return;
			var jointHandler = _juntasDoCoppeliaSim.FirstOrDefault(j => j.Nome == anguloParaJunta.Nome);
			if (jointHandler is null)
				continue;
			idsJointHandles.Add(jointHandler.Id);
			angulos.Add(anguloParaJunta.Angulo);
		}
		CoppeliaApi.MoveJoint(_coppelia, idsJointHandles.ToArray(), angulos.ToArray(), true, idsJointHandles.Count());
	}

	public async Task ComandarJuntasPorSequenciaDeComandosAsync(SequenciaDeComandosDasJuntas sequencia)
	{

		ICollection<ComandosJuntas> comandos = sequencia.ComandosJuntas;

		string nome = sequencia.Nome;
		if (nome == _instrucao.Nome || _instrucao.Nome == "Parar")
			return;

		int passos = sequencia.Passos ?? 12;
		int delayEntrePassos = sequencia.Delay ?? 17;
		int repeticoes = sequencia.Repeticoes ?? 1;
		repeticoes = Math.Min(10, repeticoes);

		_instrucao.Nome = nome;
		_instrucao.Data = DateTime.Now;

		while (nome == _instrucao.Nome && DateTime.Now < _instrucao.Data.AddSeconds(4))
		{
			foreach (ComandosJuntas comando in comandos)
			{

				if (nome != _instrucao.Nome || DateTime.Now > _instrucao.Data.AddSeconds(4))
					break;

				List<int> idsJointHandles = [];
				List<float> angulosDestino = [];
				List<float> angulosIniciais = [];

				var angulosParaJuntas = comando.AngulosParaJuntas;
				foreach (var anguloParaJunta in angulosParaJuntas)
				{
					if (nome != _instrucao.Nome || DateTime.Now > _instrucao.Data.AddSeconds(4))
						break;

					var jointHandler = _juntasDoCoppeliaSim.FirstOrDefault(j => j.Nome == anguloParaJunta.Nome);
					if (jointHandler is null)
						continue;

					idsJointHandles.Add(jointHandler.Id);

					float anguloDestino = anguloParaJunta.Angulo;
					angulosDestino.Add(anguloDestino);

					if (!_ultimoAnguloPorJoint.TryGetValue(anguloParaJunta.Nome, out float anguloInicial))
						anguloInicial = _juntasDoCoppeliaSim.FirstOrDefault(j => j.Nome == anguloParaJunta.Nome)?.AnguloInicial ?? 90;//TODO: se não tiver valor inicial,pegar o valor nos parametros da junta

					angulosIniciais.Add(anguloInicial);

					// Atualiza o último ângulo destino (será usado no próximo ciclo)
					_ultimoAnguloPorJoint[anguloParaJunta.Nome] = anguloDestino;
				}

				for (int k = 0; k < idsJointHandles.Count; k++)
				{
					string nomeDaJunta = _juntasDoCoppeliaSim.FirstOrDefault(j => j.Id == idsJointHandles[k]).Nome;

					if (nomeDaJunta == null)
						throw new Exception();

					if (!VericarSeOAnguloEstaDentroDoLimite(nomeDaJunta, angulosDestino[k]))
						throw new Exception();

				}

				for (int passo = 1; passo <= passos; passo++)
				{
					float[] angulos = new float[angulosDestino.Count];

					for (int j = 0; j < angulosDestino.Count; j++)
					{
						float inicio = angulosIniciais[j];
						float fim = angulosDestino[j];
						angulos[j] = inicio + (fim - inicio) * passo / passos;
					}

					CoppeliaApi.MoveJoint(_coppelia, idsJointHandles.ToArray(), angulos, true, idsJointHandles.Count);
					await Task.Delay(delayEntrePassos);
				}
			}
		}
	}
}
