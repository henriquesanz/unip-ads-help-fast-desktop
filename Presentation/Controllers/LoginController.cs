using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HelpFastDesktop.Presentation.Controllers;

public class LoginController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly NavigationController _navigationController;
    private LoginModel _model;

    public LoginController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _navigationController = new NavigationController(serviceProvider);
        _model = new LoginModel();
    }

    public LoginModel GetModel() => _model;

    public void SetEmail(string email)
    {
        _model.Email = email;
    }

    public void SetSenha(string senha)
    {
        _model.Senha = senha;
    }

    public async System.Threading.Tasks.Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Email) || string.IsNullOrWhiteSpace(_model.Senha))
        {
            _model.ErrorMessage = "Por favor, preencha todos os campos.";
            _model.IsLoading = false;
            return;
        }

        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            var loginSucesso = await _sessionService.FazerLoginAsync(_model.Email, _model.Senha);
            
            if (loginSucesso)
            {
                NavigateToDashboard();
            }
            else
            {
                _model.ErrorMessage = "E-mail ou senha incorretos.";
                _model.IsLoading = false;
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao fazer login: {ex.Message}";
            _model.IsLoading = false;
        }
    }

    public void NavigateToCadastro()
    {
        _navigationController.ShowCadastroUsuario();
    }

    private void NavigateToDashboard()
    {
        _model.IsLoading = false;
        _navigationController.ShowDashboard();
        OnLoginSuccessful?.Invoke();
    }

    public event Action? OnLoginSuccessful;
}
