using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Core.Models;

public class DashboardModel : INotifyPropertyChanged
{
    private string _nomeUsuario = string.Empty;
    private string _tipoUsuario = string.Empty;
    private string _descricaoTipoUsuario = string.Empty;
    private ObservableCollection<DashboardSection> _sections = new();

    public string NomeUsuario
    {
        get => _nomeUsuario;
        set
        {
            _nomeUsuario = value;
            OnPropertyChanged();
        }
    }

    public string TipoUsuario
    {
        get => _tipoUsuario;
        set
        {
            _tipoUsuario = value;
            OnPropertyChanged();
        }
    }

    public string DescricaoTipoUsuario
    {
        get => _descricaoTipoUsuario;
        set
        {
            _descricaoTipoUsuario = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardSection> Sections
    {
        get => _sections;
        set
        {
            _sections = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DashboardSection
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<DashboardAction> Actions { get; set; } = new();
}

public class DashboardAction
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public System.Windows.Input.ICommand Command { get; set; } = null!;
}

