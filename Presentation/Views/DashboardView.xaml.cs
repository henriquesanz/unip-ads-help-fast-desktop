using HelpFastDesktop.Presentation.ViewModels;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class DashboardView : Window
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribir aos eventos do ViewModel
        viewModel.LogoutRequested += OnLogoutRequested;
        viewModel.NavigateToFormRequested += OnNavigateToFormRequested;
    }

    private void OnLogoutRequested()
    {
        var result = MessageBox.Show("Deseja realmente sair do sistema?", "Logout", 
                                   MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            var loginViewModel = new LoginViewModel(App.ServiceProvider!.GetRequiredService<ISessionService>());
            var loginView = new LoginView(loginViewModel);
            loginView.Show();
            this.Close();
        }
    }

    private void OnNavigateToFormRequested(string formName)
    {
        // Por enquanto, vamos apenas mostrar uma mensagem
        MessageBox.Show($"Navegando para: {formName}", "Navegação", 
                       MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
