using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace HelpFastDesktop.Presentation.Views;

public partial class GerenciarUsuariosView : Window
{
    private readonly GerenciarUsuariosController _controller;

    public GerenciarUsuariosView(GerenciarUsuariosController controller)
    {
        InitializeComponent();
        _controller = controller;
        DataContext = _controller.GetModel();

        Loaded += GerenciarUsuariosView_Loaded;
    }

    private async void GerenciarUsuariosView_Loaded(object sender, RoutedEventArgs e)
    {
        await _controller.CarregarUsuariosAsync();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is GerenciarUsuariosModel model && model.UsuarioSelecionado != null)
        {
            _controller.SelecionarUsuario(model.UsuarioSelecionado);
        }
    }

    private async void EditarButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GerenciarUsuariosModel model || model.UsuarioSelecionado == null)
        {
            MessageBox.Show("Selecione um usuário para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (App.ServiceProvider == null)
        {
            MessageBox.Show("Serviços da aplicação não estão disponíveis.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var usuarioParaEditar = model.UsuarioSelecionado;
        var cadastroController = new CadastroUsuarioController(
            App.ServiceProvider,
            usuarioParaEditar.TipoUsuario,
            usuarioParaEditar);

        var cadastroView = new CadastroUsuarioView(cadastroController)
        {
            Owner = this
        };

        var resultado = cadastroView.ShowDialog();

        if (resultado == true)
        {
            var usuarioId = usuarioParaEditar.Id;
            await _controller.CarregarUsuariosAsync();

            var usuarioAtualizado = _controller
                .GetModel()
                .Usuarios
                .FirstOrDefault(u => u.Id == usuarioId);

            if (usuarioAtualizado != null)
            {
                _controller.GetModel().UsuarioSelecionado = usuarioAtualizado;
            }

            MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void ExcluirButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GerenciarUsuariosModel model || model.UsuarioSelecionado == null)
        {
            MessageBox.Show("Selecione um usuário para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var usuario = model.UsuarioSelecionado;
        var confirmacao = MessageBox.Show(
            $"Tem certeza de que deseja excluir o usuário '{usuario.Nome}'?",
            "Confirmar exclusão",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacao != MessageBoxResult.Yes)
        {
            return;
        }

        var sucesso = await _controller.ExcluirUsuarioSelecionadoAsync();

        if (sucesso)
        {
            MessageBox.Show("Usuário excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var mensagemErro = string.IsNullOrWhiteSpace(model.ErrorMessage)
                ? "Não foi possível excluir o usuário selecionado."
                : model.ErrorMessage;

            MessageBox.Show(mensagemErro, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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

