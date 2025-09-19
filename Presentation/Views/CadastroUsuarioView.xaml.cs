using HelpFastDesktop.Presentation.ViewModels;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class CadastroUsuarioView : Window
{
    public CadastroUsuarioView(CadastroUsuarioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribir aos eventos do ViewModel
        viewModel.SalvarSuccessful += OnSalvarSuccessful;
        viewModel.CancelarRequested += OnCancelarRequested;
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

    private void SenhaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroUsuarioViewModel viewModel)
        {
            viewModel.Senha = SenhaPasswordBox.Password;
        }
    }
}
