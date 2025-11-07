using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class CadastroUsuarioView : Window
{
    private readonly CadastroUsuarioController _controller;

    public CadastroUsuarioView(CadastroUsuarioController controller)
    {
        InitializeComponent();
        _controller = controller;
        
        // Usar o Model como DataContext
        DataContext = _controller.GetModel();
        
        // Subscribir aos eventos do Controller
        _controller.OnSalvarSuccessful += OnSalvarSuccessful;
        _controller.OnCancelarRequested += OnCancelarRequested;
    }

    private void OnSalvarSuccessful()
    {
        MessageBox.Show("Usuário salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void OnCancelarRequested()
    {
        DialogResult = false;
        Close();
    }

    private void NomeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _controller.SetNome(NomeTextBox.Text);
    }

    private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _controller.SetEmail(EmailTextBox.Text);
    }

    private void TelefoneTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _controller.SetTelefone(TelefoneTextBox.Text);
    }

    private void SenhaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _controller.SetSenha(SenhaPasswordBox.Password);
    }

    private void TipoUsuarioComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TipoUsuarioComboBox.SelectedItem is Core.Models.Entities.UserRole tipoUsuario)
        {
            _controller.SetTipoUsuarioSelecionado(tipoUsuario);
        }
    }

    private void SalvarButton_Click(object sender, RoutedEventArgs e)
    {
        _ = _controller.SalvarAsync();
    }

    private void CancelarButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.Cancelar();
    }
}
