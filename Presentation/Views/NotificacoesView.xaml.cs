using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace HelpFastDesktop.Presentation.Views;

public partial class NotificacoesView : Window
{
    private readonly NotificacoesController _controller;

    public NotificacoesView(NotificacoesController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += NotificacoesView_Loaded;
    }

    private async void NotificacoesView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarNotificacoesAsync();
    }

    private async void MarcarLidaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Notificacao notificacao)
        {
            await _controller.MarcarComoLidaAsync(notificacao);
        }
    }

    private async void MarcarTodasButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.MarcarTodasComoLidasAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarNotificacoesAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

