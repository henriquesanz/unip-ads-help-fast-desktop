using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class AuditoriaView : Window
{
    private readonly AuditoriaController _controller;

    public AuditoriaView(AuditoriaController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += AuditoriaView_Loaded;
    }

    private async void AuditoriaView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarLogsAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarLogsAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

