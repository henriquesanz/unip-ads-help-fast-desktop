using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class ConfigNotificacoesView : Window
{
    private readonly ConfigNotificacoesController _controller;

    public ConfigNotificacoesView(ConfigNotificacoesController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnConfiguracaoSalva += OnConfiguracaoSalva;
        
        Loaded += ConfigNotificacoesView_Loaded;
    }

    private async void ConfigNotificacoesView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarConfiguracaoAsync();
    }

    private void OnConfiguracaoSalva()
    {
        MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void SalvarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.SalvarConfiguracaoAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

