using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class ChamadosAtribuidosController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private ChamadosAtribuidosModel _model;

    public ChamadosAtribuidosController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _chamadoService = serviceProvider.GetRequiredService<IChamadoService>();
        _model = new ChamadosAtribuidosModel();
    }

    public ChamadosAtribuidosModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarChamadosAsync()
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

            var chamados = await _chamadoService.ListarChamadosAtribuidosAsync(usuario.Id);
            
            _model.Chamados.Clear();
            foreach (var chamado in chamados)
            {
                _model.Chamados.Add(chamado);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar chamados: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task ResolverChamadoAsync(Chamado chamado)
    {
        try
        {
            _model.IsLoading = true;
            await _chamadoService.ResolverChamadoAsync(chamado.Id, "Resolvido pelo técnico");
            await CarregarChamadosAsync();
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao resolver chamado: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public void SelecionarChamado(Chamado chamado)
    {
        _model.ChamadoSelecionado = chamado;
        OnChamadoSelecionado?.Invoke(chamado);
    }

    public event Action<Chamado>? OnChamadoSelecionado;
}

public class ChamadosAtribuidosModel : INotifyPropertyChanged
{
    private ObservableCollection<Chamado> _chamados = new();
    private Chamado? _chamadoSelecionado;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public ObservableCollection<Chamado> Chamados
    {
        get => _chamados;
        set
        {
            _chamados = value;
            OnPropertyChanged();
        }
    }

    public Chamado? ChamadoSelecionado
    {
        get => _chamadoSelecionado;
        set
        {
            _chamadoSelecionado = value;
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

