using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class ChatIAController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IAIService _aiService;
    private ChatIAModel _model;
    private Chamado? _chamadoContexto;

    public ChatIAController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _aiService = serviceProvider.GetRequiredService<IAIService>();
        _model = new ChatIAModel();
    }

    public ChatIAModel GetModel() => _model;

    public async System.Threading.Tasks.Task InicializarParaChamadoAsync(Chamado chamado)
    {
        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;
            _model.Mensagens.Clear();
            _model.MensagemAtual = string.Empty;
            _model.NumeroInteracoes = 0;
            _model.FoiDirecionadoParaTecnico = false;

            _chamadoContexto = chamado;
            _model.ChamadoId = chamado.Id;
            _model.ChamadoMotivo = chamado.Motivo;
            _model.ChamadoAssunto = ExtrairAssuntoDoChamado(chamado.Motivo);

            var usuario = _sessionService.UsuarioLogado;
            if (usuario == null)
            {
                _model.ErrorMessage = "Usuário não autenticado.";
                return;
            }

            var mensagemContexto = MontarMensagemDeContexto(chamado);
            var response = await _aiService.ProcessarMensagemChatAsync(usuario.Id, mensagemContexto, chamado.Id);

            if (response != null)
            {
                var mensagemIA = new ChatMensagem
                {
                    Texto = response.Resposta ?? "Estou com dificuldade para analisar o chamado no momento. Por favor, tente novamente em instantes.",
                    EhUsuario = false,
                    DataEnvio = DateTime.Now
                };
                _model.Mensagens.Add(mensagemIA);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao iniciar o chat: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task EnviarMensagemAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.MensagemAtual))
        {
            return;
        }

        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            var usuario = _sessionService.UsuarioLogado;
            if (usuario == null)
            {
                _model.ErrorMessage = "Usuário não autenticado.";
                return;
            }

            // Verificar se já foi direcionado para técnico
            if (_model.FoiDirecionadoParaTecnico)
            {
                var mensagemTexto = _model.MensagemAtual;
                _model.MensagemAtual = string.Empty;

                // Adicionar mensagem do usuário
                var mensagemUsuario = new ChatMensagem
                {
                    Texto = mensagemTexto,
                    EhUsuario = true,
                    DataEnvio = DateTime.Now
                };
                _model.Mensagens.Add(mensagemUsuario);

                // Salvar mensagem do usuário mesmo após direcionamento (sem chamar IA)
                if (_chamadoContexto != null)
                {
                    await _aiService.SalvarMensagemChatSimplesAsync(_chamadoContexto.Id, usuario.Id, mensagemTexto, "IA_User");
                }

                // Retornar mensagem padrão sem enviar para OpenAI
                var mensagemIA = new ChatMensagem
                {
                    Texto = "Aguarde, estamos te direcionando para um técnico, em breve você será respondido.",
                    EhUsuario = false,
                    DataEnvio = DateTime.Now
                };
                _model.Mensagens.Add(mensagemIA);
                return;
            }

            // Adicionar mensagem do usuário
            var mensagemUsuarioNormal = new ChatMensagem
            {
                Texto = _model.MensagemAtual,
                EhUsuario = true,
                DataEnvio = DateTime.Now
            };
            _model.Mensagens.Add(mensagemUsuarioNormal);

            var mensagemParaEnviar = MontarMensagemDeUsuario(_model.MensagemAtual);
            _model.MensagemAtual = string.Empty;

            // Incrementar contador de interações antes de processar
            _model.NumeroInteracoes++;

            // Verificar se já atingiu 3 interações
            if (_model.NumeroInteracoes >= 3)
            {
                // Marcar como direcionado
                _model.FoiDirecionadoParaTecnico = true;

                // Exibir mensagem de direcionamento para técnico
                var mensagemDirecionamento = new ChatMensagem
                {
                    Texto = "Seu atendimento foi direcionado para um técnico. Em breve você será respondido.",
                    EhUsuario = false,
                    DataEnvio = DateTime.Now
                };
                _model.Mensagens.Add(mensagemDirecionamento);
                return;
            }

            // Processar com IA apenas se ainda não atingiu 3 interações
            int? chamadoIdParaIA = _chamadoContexto?.Id;
            var response = await _aiService.ProcessarMensagemChatAsync(usuario.Id, mensagemParaEnviar, chamadoIdParaIA);

            if (response != null)
            {
                var mensagemIA = new ChatMensagem
                {
                    Texto = response.Resposta ?? "Desculpe, não consegui processar sua mensagem.",
                    EhUsuario = false,
                    DataEnvio = DateTime.Now
                };
                _model.Mensagens.Add(mensagemIA);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao processar mensagem: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public void SetMensagemAtual(string mensagem)
    {
        _model.MensagemAtual = mensagem;
    }

    private string MontarMensagemDeUsuario(string mensagemUsuario)
    {
        if (_chamadoContexto == null)
        {
            return mensagemUsuario;
        }

        var assunto = _model.ChamadoAssunto;
        return $"[Chamado #{_chamadoContexto.Id} - Assunto: {assunto}] {mensagemUsuario}";
    }

    private string MontarMensagemDeContexto(Chamado chamado)
    {
        var assunto = ExtrairAssuntoDoChamado(chamado.Motivo);
        var detalhes = chamado.Motivo?.Trim() ?? "Motivo não informado.";

        return $"Estou analisando o chamado #{chamado.Id} com o assunto \"{assunto}\". Os detalhes informados pelo cliente foram: {detalhes}. Forneça uma orientação inicial personalizada com base nesse contexto.";
    }

    private string ExtrairAssuntoDoChamado(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return "Assunto não informado";
        }

        var linhas = motivo.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        return linhas.Length > 0 ? linhas[0].Trim() : motivo.Trim();
    }
}

public class ChatMensagem
{
    public string Texto { get; set; } = string.Empty;
    public bool EhUsuario { get; set; }
    public DateTime DataEnvio { get; set; }
}

public class ChatIAModel : INotifyPropertyChanged
{
    private ObservableCollection<ChatMensagem> _mensagens = new();
    private string _mensagemAtual = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private int? _chamadoId;
    private string _chamadoAssunto = string.Empty;
    private string _chamadoMotivo = string.Empty;
    private int _numeroInteracoes = 0;
    private bool _foiDirecionadoParaTecnico = false;

    public ObservableCollection<ChatMensagem> Mensagens
    {
        get => _mensagens;
        set
        {
            _mensagens = value;
            OnPropertyChanged();
        }
    }

    public string MensagemAtual
    {
        get => _mensagemAtual;
        set
        {
            _mensagemAtual = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public int? ChamadoId
    {
        get => _chamadoId;
        set
        {
            _chamadoId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PossuiChamadoContexto));
        }
    }

    public string ChamadoAssunto
    {
        get => _chamadoAssunto;
        set
        {
            _chamadoAssunto = value;
            OnPropertyChanged();
        }
    }

    public string ChamadoMotivo
    {
        get => _chamadoMotivo;
        set
        {
            _chamadoMotivo = value;
            OnPropertyChanged();
        }
    }

    public int NumeroInteracoes
    {
        get => _numeroInteracoes;
        set
        {
            _numeroInteracoes = value;
            OnPropertyChanged();
        }
    }

    public bool FoiDirecionadoParaTecnico
    {
        get => _foiDirecionadoParaTecnico;
        set
        {
            _foiDirecionadoParaTecnico = value;
            OnPropertyChanged();
        }
    }

    public bool PossuiChamadoContexto => ChamadoId.HasValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

