using HelpFastDesktop.Core.Models.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HelpFastDesktop.Presentation.Views;

public partial class FAQExpanderControl : UserControl
{
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(FAQExpanderControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty FAQItemProperty =
        DependencyProperty.Register(nameof(FAQItem), typeof(FAQItem), typeof(FAQExpanderControl));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public FAQItem? FAQItem
    {
        get => (FAQItem?)GetValue(FAQItemProperty);
        set => SetValue(FAQItemProperty, value);
    }

    public event RoutedEventHandler? Click;

    public FAQExpanderControl()
    {
        InitializeComponent();
        DataContextChanged += FAQExpanderControl_DataContextChanged;
    }

    private void FAQExpanderControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is FAQItem faqItem)
        {
            FAQItem = faqItem;
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Click?.Invoke(this, new RoutedEventArgs());
    }
}

