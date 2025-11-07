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

    public ChatIAController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _aiService = serviceProvider.GetRequiredService<IAIService>();
        _model = new ChatIAModel();
    }

    public ChatIAModel GetModel() => _model;

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

            // Adicionar mensagem do usuário
            var mensagemUsuario = new ChatMensagem
            {
                Texto = _model.MensagemAtual,
                EhUsuario = true,
                DataEnvio = DateTime.Now
            };
            _model.Mensagens.Add(mensagemUsuario);

            var mensagemParaEnviar = _model.MensagemAtual;
            _model.MensagemAtual = string.Empty;

            // Processar com IA
            var response = await _aiService.ProcessarMensagemChatAsync(usuario.Id, mensagemParaEnviar);

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

