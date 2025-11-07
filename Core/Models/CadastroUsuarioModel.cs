using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HelpFastDesktop.Core.Models.Entities;

namespace HelpFastDesktop.Core.Models;

public class CadastroUsuarioModel : INotifyPropertyChanged
{
    private string _nome = string.Empty;
    private string _email = string.Empty;
    private string _telefone = string.Empty;
    private string _senha = string.Empty;
    private UserRole _tipoUsuarioSelecionado = UserRole.Cliente;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private bool _isEditando = false;
    private string _tituloFormulario = string.Empty;
    private string _textoBotaoSalvar = string.Empty;
    private bool _mostrarTipoUsuario = false;
    private ObservableCollection<UserRole> _tiposUsuarioDisponiveis = new();

    public string Nome
    {
        get => _nome;
        set
        {
            _nome = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    public string Telefone
    {
        get => _telefone;
        set
        {
            _telefone = value;
            OnPropertyChanged();
        }
    }

    public string Senha
    {
        get => _senha;
        set
        {
            _senha = value;
            OnPropertyChanged();
        }
    }

    public UserRole TipoUsuarioSelecionado
    {
        get => _tipoUsuarioSelecionado;
        set
        {
            _tipoUsuarioSelecionado = value;
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

    public bool IsEditando
    {
        get => _isEditando;
        set
        {
            _isEditando = value;
            OnPropertyChanged();
        }
    }

    public string TituloFormulario
    {
        get => _tituloFormulario;
        set
        {
            _tituloFormulario = value;
            OnPropertyChanged();
        }
    }

    public string TextoBotaoSalvar
    {
        get => _textoBotaoSalvar;
        set
        {
            _textoBotaoSalvar = value;
            OnPropertyChanged();
        }
    }

    public bool MostrarTipoUsuario
    {
        get => _mostrarTipoUsuario;
        set
        {
            _mostrarTipoUsuario = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<UserRole> TiposUsuarioDisponiveis
    {
        get => _tiposUsuarioDisponiveis;
        set
        {
            _tiposUsuarioDisponiveis = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


