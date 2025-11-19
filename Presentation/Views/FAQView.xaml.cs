using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop;
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
        
        Loaded += FAQView_Loaded;
    }

    private async void FAQView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarFAQsAsync();
        AtualizarEstadoExpandido();
        
        if (DataContext is FAQModel model)
        {
            model.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(FAQModel.FAQExpandidoId) || args.PropertyName == nameof(FAQModel.FAQs))
                {
                    AtualizarEstadoExpandido();
                }
            };
        }
    }

    private void AtualizarEstadoExpandido()
    {
        if (DataContext is FAQModel model)
        {
            FAQItem1.IsExpanded = model.FAQExpandidoId == FAQItem1.FAQItem?.Id;
            FAQItem2.IsExpanded = model.FAQExpandidoId == FAQItem2.FAQItem?.Id;
            FAQItem3.IsExpanded = model.FAQExpandidoId == FAQItem3.FAQItem?.Id;
            FAQItem4.IsExpanded = model.FAQExpandidoId == FAQItem4.FAQItem?.Id;
            
            // Atualizar visibilidade dos controles baseado na disponibilidade de FAQs
            FAQItem1.Visibility = model.FAQs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FAQItem2.Visibility = model.FAQs.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            FAQItem3.Visibility = model.FAQs.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            FAQItem4.Visibility = model.FAQs.Count > 3 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void FAQItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FAQExpanderControl expander && expander.FAQItem != null)
        {
            if (DataContext is FAQModel model)
            {
                model.ToggleFAQExpandido(expander.FAQItem.Id);
                AtualizarEstadoExpandido();
            }
        }
    }

    private async void ChatIAButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.ServiceProvider == null)
        {
            MessageBox.Show("Serviços da aplicação não estão disponíveis.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var chatController = new ChatIAController(App.ServiceProvider);
            
            // Tentar buscar o último chamado aberto do usuário
            var ultimoChamado = await _controller.ObterUltimoChamadoAbertoAsync();
            
            if (ultimoChamado != null)
            {
                // Inicializar o chat com o último chamado
                await chatController.InicializarParaChamadoAsync(ultimoChamado);
            }

            var chatView = new ChatIAView(chatController)
            {
                Owner = this
            };

            chatView.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir o Chat com IA: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FecharButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

