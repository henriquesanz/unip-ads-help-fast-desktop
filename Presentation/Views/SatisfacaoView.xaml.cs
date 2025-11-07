using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class SatisfacaoView : Window
{
    private readonly SatisfacaoController _controller;

    public SatisfacaoView(SatisfacaoController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += SatisfacaoView_Loaded;
    }

    private async void SatisfacaoView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarSatisfacaoAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarSatisfacaoAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

