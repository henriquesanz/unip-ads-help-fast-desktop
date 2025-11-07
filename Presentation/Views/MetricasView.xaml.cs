using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class MetricasView : Window
{
    private readonly MetricasController _controller;

    public MetricasView(MetricasController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += MetricasView_Loaded;
    }

    private async void MetricasView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarMetricasAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarMetricasAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

