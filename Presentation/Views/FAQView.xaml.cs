using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HelpFastDesktop.Presentation.Views;

public partial class FAQView : Window
{
    private readonly FAQController _controller;

    public FAQView(FAQController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnFAQSelecionado += OnFAQSelecionado;
        
        Loaded += FAQView_Loaded;
    }

    private async void FAQView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarFAQsAsync();
    }

    private void OnFAQSelecionado(FAQItem faq)
    {
        // FAQ já está sendo exibido na lista
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FAQModel model && model.FAQSelecionado != null)
        {
            _controller.SelecionarFAQ(model.FAQSelecionado);
        }
    }

    private async void BuscarButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is FAQModel model)
        {
            await _controller.BuscarAsync(model.TermoBusca);
        }
    }

    private async void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is FAQModel model)
        {
            await _controller.BuscarAsync(model.TermoBusca);
        }
    }

    private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarFAQsAsync();
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

