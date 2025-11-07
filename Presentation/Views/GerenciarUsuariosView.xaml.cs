using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace HelpFastDesktop.Presentation.Views;

public partial class GerenciarUsuariosView : Window
{
    private readonly GerenciarUsuariosController _controller;

    public GerenciarUsuariosView(GerenciarUsuariosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();
        
        _controller.OnUsuarioSelecionado += OnUsuarioSelecionado;
        
        Loaded += GerenciarUsuariosView_Loaded;
    }

    private async void GerenciarUsuariosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarUsuariosAsync();
    }

    private void OnUsuarioSelecionado(Usuario usuario)
    {
        MessageBox.Show($"Usuário: {usuario.Nome}\nEmail: {usuario.Email}\nTipo: {usuario.TipoUsuarioDisplay}", 
            "Detalhes do Usuário", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is GerenciarUsuariosModel model && model.UsuarioSelecionado != null)
        {
            _controller.SelecionarUsuario(model.UsuarioSelecionado);
        }
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

