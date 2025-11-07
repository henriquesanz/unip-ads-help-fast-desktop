using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class NovoChamadoController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private readonly INotificacaoService _notificacaoService;
    private NovoChamadoModel _model;

    public NovoChamadoController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _chamadoService = serviceProvider.GetRequiredService<IChamadoService>();
        _notificacaoService = serviceProvider.GetRequiredService<INotificacaoService>();
        _model = new NovoChamadoModel();
    }

    public NovoChamadoModel GetModel() => _model;

    public void SetMotivo(string motivo)
    {
        _model.Motivo = motivo;
    }

    public async System.Threading.Tasks.Task CriarChamadoAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Motivo))
        {
            _model.ErrorMessage = "O motivo do chamado é obrigatório.";
            return;
        }

        if (_model.Motivo.Length < 10)
        {
            _model.ErrorMessage = "O motivo deve ter pelo menos 10 caracteres.";
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

            var chamado = new Chamado
            {
                Motivo = _model.Motivo.Trim(),
                ClienteId = usuario.Id,
                Status = "Aberto",
                DataAbertura = DateTime.Now
            };

            var chamadoCriado = await _chamadoService.CriarChamadoAsync(chamado);
            
            // Notificar criação do chamado
            await _notificacaoService.NotificarChamadoCriadoAsync(chamadoCriado);

            OnChamadoCriado?.Invoke(chamadoCriado);
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao criar chamado: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public void Cancelar()
    {
        OnCancelarRequested?.Invoke();
    }

    public event Action<Chamado>? OnChamadoCriado;
    public event Action? OnCancelarRequested;
}

public class NovoChamadoModel : INotifyPropertyChanged
{
    private string _motivo = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public string Motivo
    {
        get => _motivo;
        set
        {
            _motivo = value;
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

