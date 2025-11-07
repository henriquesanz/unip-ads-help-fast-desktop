using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class PermissoesView : Window
{
    private readonly PermissoesController _controller;

    public PermissoesView(PermissoesController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += PermissoesView_Loaded;
    }

    private async void PermissoesView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarUsuariosAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarUsuariosAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

