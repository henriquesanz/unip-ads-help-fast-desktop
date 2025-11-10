using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop;
using System.Windows;
using System.Windows.Controls;
using System;

namespace HelpFastDesktop.Presentation.Views;

public partial class MeusChamadosView : Window
{
    private readonly MeusChamadosController _controller;

    public MeusChamadosView(MeusChamadosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnChatSolicitado += OnChatSolicitado;
        
        Loaded += MeusChamadosView_Loaded;
    }

    private async void MeusChamadosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarChamadosAsync();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MeusChamadosModel model && model.ChamadoSelecionado != null)
        {
            _controller.SelecionarChamado(model.ChamadoSelecionado);
        }
    }

    private void ChatIAButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.SolicitarChatParaChamadoSelecionado();
    }

    private async void OnChatSolicitado(Chamado chamado)
    {
        if (App.ServiceProvider == null)
        {
            MessageBox.Show("Serviços da aplicação não estão disponíveis.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var chatController = new ChatIAController(App.ServiceProvider);
            await chatController.InicializarParaChamadoAsync(chamado);

            var chatView = new ChatIAView(chatController)
            {
                Owner = this
            };

            chatView.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir o chat com a IA: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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

