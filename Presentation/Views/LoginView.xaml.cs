using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class LoginView : Window
{
    private readonly LoginController _controller;

    public LoginView(LoginController controller)
    {
        InitializeComponent();
        _controller = controller;
        
        // Usar o Model como DataContext
        DataContext = _controller.GetModel();
        
        // Subscribir aos eventos do Controller
        _controller.OnLoginSuccessful += OnLoginSuccessful;
    }

    private void OnLoginSuccessful()
    {
        this.Close();
    }

    private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _controller.SetEmail(EmailTextBox.Text);
    }

    private void SenhaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _controller.SetSenha(SenhaPasswordBox.Password);
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _ = _controller.LoginAsync();
    }

    private void CadastrarButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.NavigateToCadastro();
    }
}
