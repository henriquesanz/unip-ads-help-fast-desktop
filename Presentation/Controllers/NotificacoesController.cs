using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class NotificacoesController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly INotificacaoService _notificacaoService;
    private NotificacoesModel _model;

    public NotificacoesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _notificacaoService = serviceProvider.GetRequiredService<INotificacaoService>();
        _model = new NotificacoesModel();
    }

    public NotificacoesModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarNotificacoesAsync()
    {
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

            var notificacoes = await _notificacaoService.ListarNotificacoesUsuarioAsync(usuario.Id);
            
            _model.Notificacoes.Clear();
            foreach (var notificacao in notificacoes)
            {
                _model.Notificacoes.Add(notificacao);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar notificações: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task MarcarComoLidaAsync(Notificacao notificacao)
    {
        try
        {
            await _notificacaoService.MarcarComoLidaAsync(notificacao.Id);
            await CarregarNotificacoesAsync();
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao marcar notificação como lida: {ex.Message}";
        }
    }

    public async System.Threading.Tasks.Task MarcarTodasComoLidasAsync()
    {
        try
        {
            var usuario = _sessionService.UsuarioLogado;
            if (usuario != null)
            {
                await _notificacaoService.MarcarTodasComoLidasAsync(usuario.Id);
                await CarregarNotificacoesAsync();
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao marcar todas como lidas: {ex.Message}";
        }
    }
}

public class NotificacoesModel : INotifyPropertyChanged
{
    private ObservableCollection<Notificacao> _notificacoes = new();
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public ObservableCollection<Notificacao> Notificacoes
    {
        get => _notificacoes;
        set
        {
            _notificacoes = value;
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

