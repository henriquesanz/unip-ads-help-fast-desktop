using HelpFastDesktop.Presentation.Commands;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Entities;
using System.Windows;
using System.Windows.Input;

namespace HelpFastDesktop.Presentation.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private string _email = string.Empty;
    private string _senha = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public LoginViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
        LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsLoading);
        CadastrarCommand = new RelayCommand(() => NavigateToCadastro());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand CadastrarCommand { get; }

    public event Action? LoginSuccessful;
    public event Action? NavigateToCadastroRequested;

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            ErrorMessage = "Por favor, preencha todos os campos.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var loginSucesso = await _sessionService.FazerLoginAsync(Email, Senha);
            
            if (loginSucesso)
            {
                LoginSuccessful?.Invoke();
            }
            else
            {
                ErrorMessage = "E-mail ou senha incorretos.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao fazer login: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NavigateToCadastro()
    {
        NavigateToCadastroRequested?.Invoke();
    }
}
