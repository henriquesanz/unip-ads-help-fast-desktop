using HelpFastDesktop.Core.Interfaces;
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

    private void RelatoriosView_Loaded(object sender, RoutedEventArgs e)
    {
        _controller.Inicializar();
    }

    private async void BaixarRelatorioButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.ExportarRelatorioMensalAsync();
    }
}

