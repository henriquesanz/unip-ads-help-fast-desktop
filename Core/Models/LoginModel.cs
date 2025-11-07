using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Core.Models;

public class LoginModel : INotifyPropertyChanged
{
    private string _email = string.Empty;
    private string _senha = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
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

