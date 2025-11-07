using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace HelpFastDesktop.Presentation.Views;

public partial class ChamadosAtribuidosView : Window
{
    private readonly ChamadosAtribuidosController _controller;

    public ChamadosAtribuidosView(ChamadosAtribuidosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChamadoSelecionado += OnChamadoSelecionado;
        
        Loaded += ChamadosAtribuidosView_Loaded;
    }

    private async void ChamadosAtribuidosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarChamadosAsync();
    }

    private void OnChamadoSelecionado(Chamado chamado)
    {
        MessageBox.Show($"Chamado #{chamado.Id}\n\nCliente: {chamado.Cliente?.Nome}\nMotivo: {chamado.Motivo}\nStatus: {chamado.StatusDisplay}", 
            "Detalhes do Chamado", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ChamadosAtribuidosModel model && model.ChamadoSelecionado != null)
        {
            _controller.SelecionarChamado(model.ChamadoSelecionado);
        }
    }

    private async void ResolverButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ChamadosAtribuidosModel model && model.ChamadoSelecionado != null)
        {
            var result = MessageBox.Show($"Deseja marcar o chamado #{model.ChamadoSelecionado.Id} como resolvido?", 
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await _controller.ResolverChamadoAsync(model.ChamadoSelecionado);
                MessageBox.Show("Chamado resolvido com sucesso!", "Sucesso", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            MessageBox.Show("Selecione um chamado primeiro.", "Aviso", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
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

