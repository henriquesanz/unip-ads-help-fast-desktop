using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class PermissoesController : BaseController
{
    private readonly IUsuarioService _usuarioService;
    private PermissoesModel _model;

    public PermissoesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _usuarioService = serviceProvider.GetRequiredService<IUsuarioService>();
        _model = new PermissoesModel();
    }

    public PermissoesModel GetModel() => _model;

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
}

public class PermissoesModel : INotifyPropertyChanged
{
    private ObservableCollection<Usuario> _usuarios = new();
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

