using HelpFastDesktop.Presentation.ViewModels;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribir aos eventos do ViewModel
        viewModel.LoginSuccessful += OnLoginSuccessful;
        viewModel.NavigateToCadastroRequested += OnNavigateToCadastroRequested;
    }

    private void OnLoginSuccessful()
    {
        // Fechar a tela de login e abrir o dashboard
        var dashboardViewModel = new DashboardViewModel(
            App.ServiceProvider!.GetRequiredService<ISessionService>(),
            App.ServiceProvider!.GetRequiredService<IChamadoService>(),
            App.ServiceProvider!.GetRequiredService<IUsuarioService>());
        var dashboardView = new DashboardView(dashboardViewModel);
        dashboardView.Show();
        this.Close();
    }

    private void OnNavigateToCadastroRequested()
    {
        // Abrir tela de cadastro
        var cadastroViewModel = new CadastroUsuarioViewModel(
            App.ServiceProvider!.GetRequiredService<ISessionService>(),
            App.ServiceProvider!.GetRequiredService<IUsuarioService>());
        var cadastroView = new CadastroUsuarioView(cadastroViewModel);
        cadastroView.ShowDialog();
    }

    private void SenhaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Senha = SenhaPasswordBox.Password;
        }
    }
}
