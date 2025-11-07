using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Input;

namespace HelpFastDesktop.Presentation.Views;

public partial class ChatIAView : Window
{
    private readonly ChatIAController _controller;

    public ChatIAView(ChatIAController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
    }

    private async void EnviarButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ChatIAModel model)
        {
            _controller.SetMensagemAtual(model.MensagemAtual);
            await _controller.EnviarMensagemAsync();
        }
    }

    private async void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is ChatIAModel model)
        {
            _controller.SetMensagemAtual(model.MensagemAtual);
            await _controller.EnviarMensagemAsync();
        }
    }
}

