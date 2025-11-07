using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class AtribuirChamadosController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private readonly IUsuarioService _usuarioService;
    private readonly INotificacaoService _notificacaoService;
    private AtribuirChamadosModel _model;

    public AtribuirChamadosController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _chamadoService = serviceProvider.GetRequiredService<IChamadoService>();
        _usuarioService = serviceProvider.GetRequiredService<IUsuarioService>();
        _notificacaoService = serviceProvider.GetRequiredService<INotificacaoService>();
        _model = new AtribuirChamadosModel();
    }

    public AtribuirChamadosModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarDadosAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            var chamados = await _chamadoService.ListarChamadosPorStatusAsync("Aberto");
            var tecnicos = await _usuarioService.ListarTecnicosAsync();
            
            _model.Chamados.Clear();
            foreach (var chamado in chamados)
            {
                _model.Chamados.Add(chamado);
            }

            _model.Tecnicos.Clear();
            foreach (var tecnico in tecnicos)
            {
                _model.Tecnicos.Add(tecnico);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar dados: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task AtribuirChamadoAsync()
    {
        if (_model.ChamadoSelecionado == null)
        {
            _model.ErrorMessage = "Selecione um chamado.";
            return;
        }

        if (_model.TecnicoSelecionado == null)
        {
            _model.ErrorMessage = "Selecione um técnico.";
            return;
        }

        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            var chamado = await _chamadoService.AtribuirChamadoAsync(
                _model.ChamadoSelecionado.Id, 
                _model.TecnicoSelecionado.Id);

            var chamadoCompleto = await _chamadoService.ObterPorIdAsync(chamado.Id);
            if (chamadoCompleto != null)
            {
                await _notificacaoService.NotificarChamadoAtribuidoAsync(chamadoCompleto, _model.TecnicoSelecionado);
            }

            await CarregarDadosAsync();
            OnChamadoAtribuido?.Invoke();
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao atribuir chamado: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public event Action? OnChamadoAtribuido;
}

public class AtribuirChamadosModel : INotifyPropertyChanged
{
    private ObservableCollection<Chamado> _chamados = new();
    private ObservableCollection<Usuario> _tecnicos = new();
    private Chamado? _chamadoSelecionado;
    private Usuario? _tecnicoSelecionado;
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

    public ObservableCollection<Usuario> Tecnicos
    {
        get => _tecnicos;
        set
        {
            _tecnicos = value;
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

    public Usuario? TecnicoSelecionado
    {
        get => _tecnicoSelecionado;
        set
        {
            _tecnicoSelecionado = value;
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

