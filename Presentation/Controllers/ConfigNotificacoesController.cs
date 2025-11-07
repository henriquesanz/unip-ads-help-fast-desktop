using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class ConfigNotificacoesController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly INotificacaoService _notificacaoService;
    private ConfigNotificacoesModel _model;

    public ConfigNotificacoesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _notificacaoService = serviceProvider.GetRequiredService<INotificacaoService>();
        _model = new ConfigNotificacoesModel();
    }

    public ConfigNotificacoesModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarConfiguracaoAsync()
    {
        try
        {
            _model.IsLoading = true;
            var usuario = _sessionService.UsuarioLogado;
            if (usuario == null) return;

            var config = await _notificacaoService.ObterConfiguracaoUsuarioAsync(usuario.Id);
            _model.Configuracao = config;
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar configuração: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task SalvarConfiguracaoAsync()
    {
        try
        {
            _model.IsLoading = true;
            if (_model.Configuracao != null)
            {
                await _notificacaoService.AtualizarConfiguracaoAsync(_model.Configuracao);
                OnConfiguracaoSalva?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao salvar configuração: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public event Action? OnConfiguracaoSalva;
}

public class ConfigNotificacoesModel : INotifyPropertyChanged
{
    private ConfiguracaoNotificacao? _configuracao;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public ConfiguracaoNotificacao? Configuracao
    {
        get => _configuracao;
        set
        {
            _configuracao = value;
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

