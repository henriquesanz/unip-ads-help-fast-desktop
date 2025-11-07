using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace HelpFastDesktop.Presentation.Views;

public partial class TodosChamadosView : Window
{
    private readonly TodosChamadosController _controller;

    public TodosChamadosView(TodosChamadosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChamadoSelecionado += OnChamadoSelecionado;
        
        Loaded += TodosChamadosView_Loaded;
    }

    private async void TodosChamadosView_Loaded(object sender, RoutedEventArgs e)
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
        if (DataContext is TodosChamadosModel model && model.ChamadoSelecionado != null)
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

