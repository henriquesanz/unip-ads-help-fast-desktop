using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class ConfiguracoesView : Window
{
    private readonly ConfiguracoesController _controller;

    public ConfiguracoesView(ConfiguracoesController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += ConfiguracoesView_Loaded;
    }

    private async void ConfiguracoesView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarConfiguracoesAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarConfiguracoesAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

