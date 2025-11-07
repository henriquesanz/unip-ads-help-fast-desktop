using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class DashboardView : Window
{
    private readonly DashboardController _controller;

    public DashboardView(DashboardController controller)
    {
        InitializeComponent();
        _controller = controller;
        
        // Usar o Model como DataContext
        DataContext = _controller.GetModel();
        
        // Subscribir aos eventos do Controller
        _controller.OnLogoutRequested += OnLogoutRequested;
        _controller.OnNavigateToFormRequested += OnNavigateToFormRequested;
    }

    private void OnLogoutRequested()
    {
        var result = MessageBox.Show("Deseja realmente sair do sistema?", "Logout", 
                                   MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            // Voltar para a tela de login
            var navigationController = new NavigationController(App.ServiceProvider!);
            navigationController.ShowLogin(this);
        }
    }

    private void OnNavigateToFormRequested(string formName)
    {
        var navigationController = new NavigationController(App.ServiceProvider!);
        navigationController.NavigateToForm(formName, this);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.Logout();
    }
}
