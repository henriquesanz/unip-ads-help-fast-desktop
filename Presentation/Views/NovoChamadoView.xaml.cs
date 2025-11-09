using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class NovoChamadoView : Window
{
    private readonly NovoChamadoController _controller;

    public NovoChamadoView(NovoChamadoController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChamadoCriado += OnChamadoCriado;
        _controller.OnCancelarRequested += OnCancelarRequested;
    }

    private void OnChamadoCriado(Chamado chamado)
    {
        MessageBox.Show($"Chamado #{chamado.Id} criado com sucesso!", "Sucesso", 
            MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void OnCancelarRequested()
    {
        DialogResult = false;
        Close();
    }

    private void MotivoTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (DataContext is NovoChamadoModel model)
        {
            _controller.SetMotivo(model.Motivo);
        }
    }

    private void AssuntoTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (DataContext is NovoChamadoModel model)
        {
            _controller.SetAssunto(model.Assunto);
        }
    }

    private async void CriarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CriarChamadoAsync();
    }

    private void CancelarButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.Cancelar();
    }
}

