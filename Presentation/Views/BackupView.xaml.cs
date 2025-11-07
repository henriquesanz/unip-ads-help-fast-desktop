using HelpFastDesktop.Presentation.Controllers;
using System.Windows;

namespace HelpFastDesktop.Presentation.Views;

public partial class BackupView : Window
{
    private readonly BackupController _controller;

    public BackupView(BackupController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        Loaded += BackupView_Loaded;
    }

    private async void BackupView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarInfoBackupAsync();
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarInfoBackupAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

