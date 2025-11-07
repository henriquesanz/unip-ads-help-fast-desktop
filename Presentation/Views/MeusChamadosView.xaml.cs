using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace HelpFastDesktop.Presentation.Views;

public partial class MeusChamadosView : Window
{
    private readonly MeusChamadosController _controller;

    public MeusChamadosView(MeusChamadosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChamadoSelecionado += OnChamadoSelecionado;
        
        Loaded += MeusChamadosView_Loaded;
    }

    private async void MeusChamadosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarChamadosAsync();
    }

    private void OnChamadoSelecionado(Chamado chamado)
    {
        // Pode abrir uma janela de detalhes do chamado aqui
        MessageBox.Show($"Chamado #{chamado.Id}\n\nMotivo: {chamado.Motivo}\nStatus: {chamado.StatusDisplay}", 
            "Detalhes do Chamado", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MeusChamadosModel model && model.ChamadoSelecionado != null)
        {
            _controller.SelecionarChamado(model.ChamadoSelecionado);
        }
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarChamadosAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

