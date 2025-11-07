using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class AtribuirChamadosView : Window
{
    private readonly AtribuirChamadosController _controller;

    public AtribuirChamadosView(AtribuirChamadosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChamadoAtribuido += OnChamadoAtribuido;
        
        Loaded += AtribuirChamadosView_Loaded;
    }

    private async void AtribuirChamadosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarDadosAsync();
    }

    private void OnChamadoAtribuido()
    {
        MessageBox.Show("Chamado atribuído com sucesso!", "Sucesso", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void AtribuirButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.AtribuirChamadoAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarDadosAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

