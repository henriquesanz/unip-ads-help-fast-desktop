using HelpFastDesktop.Presentation.Views;
using System.Windows;

namespace HelpFastDesktop.Presentation.Controllers;

public class NavigationController : BaseController
{
    private Window? _currentWindow;

    public NavigationController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public void ShowLogin(Window? currentWindowToClose = null)
    {
        var controller = new LoginController(ServiceProvider);
        var view = new LoginView(controller);
        if (_currentWindow != null && _currentWindow != currentWindowToClose)
        {
            _currentWindow.Close();
        }

        view.Show();
        Application.Current.MainWindow = view;

        currentWindowToClose?.Close();

        _currentWindow = view;
    }

    public void ShowDashboard()
    {
        var controller = new DashboardController(ServiceProvider);
        var view = new DashboardView(controller);
        ShowWindow(view);
    }

    public void ShowCadastroUsuario()
    {
        var controller = new CadastroUsuarioController(ServiceProvider);
        var view = new CadastroUsuarioView(controller);
        var result = view.ShowDialog();
    }

    private void ShowWindow(Window window)
    {
        _currentWindow?.Close();
        _currentWindow = window;
        window.Show();
    }

    public void CloseCurrentWindow()
    {
        _currentWindow?.Close();
        _currentWindow = null;
    }

    public void NavigateToForm(string formName, Window? currentWindow = null)
    {
        try
        {
            switch (formName)
            {
                case "NovoChamado":
                    ShowNovoChamado(currentWindow);
                    break;
                case "MeusChamados":
                    ShowMeusChamados(currentWindow);
                    break;
                case "TodosChamados":
                    ShowTodosChamados(currentWindow);
                    break;
                case "ChamadosAtribuidos":
                    ShowChamadosAtribuidos(currentWindow);
                    break;
                case "AtribuirChamados":
                    ShowAtribuirChamados(currentWindow);
                    break;
                case "FAQ":
                    ShowFAQ(currentWindow);
                    break;
                case "ChatIA":
                    ShowChatIA(currentWindow);
                    break;
                case "GerenciarUsuarios":
                    ShowGerenciarUsuarios(currentWindow);
                    break;
                case "CadastrarUsuario":
                    ShowCadastroUsuario();
                    break;
                case "Relatorios":
                    ShowRelatorios(currentWindow);
                    break;
                default:
                    MessageBox.Show($"Tela '{formName}' ainda não implementada.", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir tela '{formName}': {ex.Message}", "Erro", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowNovoChamado(Window? currentWindow)
    {
        var controller = new NovoChamadoController(ServiceProvider);
        var view = new NovoChamadoView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowMeusChamados(Window? currentWindow)
    {
        var controller = new MeusChamadosController(ServiceProvider);
        var view = new MeusChamadosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowTodosChamados(Window? currentWindow)
    {
        var controller = new TodosChamadosController(ServiceProvider);
        var view = new TodosChamadosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowChamadosAtribuidos(Window? currentWindow)
    {
        var controller = new ChamadosAtribuidosController(ServiceProvider);
        var view = new ChamadosAtribuidosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowAtribuirChamados(Window? currentWindow)
    {
        var controller = new AtribuirChamadosController(ServiceProvider);
        var view = new AtribuirChamadosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowFAQ(Window? currentWindow)
    {
        var controller = new FAQController(ServiceProvider);
        var view = new FAQView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowChatIA(Window? currentWindow)
    {
        var controller = new ChatIAController(ServiceProvider);
        var view = new ChatIAView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowGerenciarUsuarios(Window? currentWindow)
    {
        var controller = new GerenciarUsuariosController(ServiceProvider);
        var view = new GerenciarUsuariosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowRelatorios(Window? currentWindow)
    {
        var controller = new RelatoriosController(ServiceProvider);
        var view = new RelatoriosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowWindowAsDialog(Window window, Window? parentWindow)
    {
        if (parentWindow != null)
        {
            window.Owner = parentWindow;
        }
        window.ShowDialog();
    }
}
