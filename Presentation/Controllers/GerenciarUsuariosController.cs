using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class GerenciarUsuariosController : BaseController
{
    private readonly IUsuarioService _usuarioService;
    private GerenciarUsuariosModel _model;

    public GerenciarUsuariosController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _usuarioService = serviceProvider.GetRequiredService<IUsuarioService>();
        _model = new GerenciarUsuariosModel();
    }

    public GerenciarUsuariosModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarUsuariosAsync()
    {
        try
        {
            _model.IsLoading = true;
            var usuarios = await _usuarioService.ListarUsuariosPorTipoAsync(UserRole.Cliente);
            usuarios.AddRange(await _usuarioService.ListarTecnicosAsync());
            usuarios.AddRange(await _usuarioService.ListarAdministradoresAsync());
            
            _model.Usuarios.Clear();
            foreach (var usuario in usuarios.OrderBy(u => u.Nome))
            {
                _model.Usuarios.Add(usuario);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar usuários: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public void SelecionarUsuario(Usuario usuario)
    {
        _model.UsuarioSelecionado = usuario;
    }

    public async System.Threading.Tasks.Task<bool> ExcluirUsuarioSelecionadoAsync()
    {
        if (_model.UsuarioSelecionado == null)
        {
            _model.ErrorMessage = "Selecione um usuário para excluir.";
            return false;
        }

        try
        {
            await _usuarioService.RemoverUsuarioAsync(_model.UsuarioSelecionado.Id);
            await CarregarUsuariosAsync();
            _model.UsuarioSelecionado = null;
            _model.ErrorMessage = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao excluir usuário: {ex.Message}";
            return false;
        }
    }

}

public class GerenciarUsuariosModel : INotifyPropertyChanged
{
    private ObservableCollection<Usuario> _usuarios = new();
    private Usuario? _usuarioSelecionado;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public ObservableCollection<Usuario> Usuarios
    {
        get => _usuarios;
        set
        {
            _usuarios = value;
            OnPropertyChanged();
        }
    }

    public Usuario? UsuarioSelecionado
    {
        get => _usuarioSelecionado;
        set
        {
            _usuarioSelecionado = value;
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

