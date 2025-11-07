using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class RelatoriosView : Window
{
    private readonly RelatoriosController _controller;

    public RelatoriosView(RelatoriosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += RelatoriosView_Loaded;
    }

    private async void RelatoriosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarRelatoriosAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarRelatoriosAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

