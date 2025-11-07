using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using HelpFastDesktop.Presentation.Controllers;

namespace HelpFastDesktop.Presentation.Controllers;

public class NavigationController : BaseController
{
    private Window? _currentWindow;

    public NavigationController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public void ShowLogin(Window? currentWindowToClose = null)
    {
        currentWindowToClose?.Close();
        var controller = new LoginController(ServiceProvider);
        var view = new LoginView(controller);
        ShowWindow(view);
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
                case "Notificacoes":
                    ShowNotificacoes(currentWindow);
                    break;
                case "ConfigNotificacoes":
                    ShowConfigNotificacoes(currentWindow);
                    break;
                case "GerenciarUsuarios":
                    ShowGerenciarUsuarios(currentWindow);
                    break;
                case "CadastrarUsuario":
                    ShowCadastroUsuario();
                    break;
                case "Permissoes":
                    ShowPermissoes(currentWindow);
                    break;
                case "Relatorios":
                    ShowRelatorios(currentWindow);
                    break;
                case "Metricas":
                    ShowMetricas(currentWindow);
                    break;
                case "Satisfacao":
                    ShowSatisfacao(currentWindow);
                    break;
                case "Configuracoes":
                    ShowConfiguracoes(currentWindow);
                    break;
                case "Auditoria":
                    ShowAuditoria(currentWindow);
                    break;
                case "Backup":
                    ShowBackup(currentWindow);
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

    private void ShowNotificacoes(Window? currentWindow)
    {
        var controller = new NotificacoesController(ServiceProvider);
        var view = new NotificacoesView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowConfigNotificacoes(Window? currentWindow)
    {
        var controller = new ConfigNotificacoesController(ServiceProvider);
        var view = new ConfigNotificacoesView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowGerenciarUsuarios(Window? currentWindow)
    {
        var controller = new GerenciarUsuariosController(ServiceProvider);
        var view = new GerenciarUsuariosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowPermissoes(Window? currentWindow)
    {
        var controller = new PermissoesController(ServiceProvider);
        var view = new PermissoesView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowRelatorios(Window? currentWindow)
    {
        var controller = new RelatoriosController(ServiceProvider);
        var view = new RelatoriosView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowMetricas(Window? currentWindow)
    {
        var controller = new MetricasController(ServiceProvider);
        var view = new MetricasView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowSatisfacao(Window? currentWindow)
    {
        var controller = new SatisfacaoController(ServiceProvider);
        var view = new SatisfacaoView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowConfiguracoes(Window? currentWindow)
    {
        var controller = new ConfiguracoesController(ServiceProvider);
        var view = new ConfiguracoesView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowAuditoria(Window? currentWindow)
    {
        var controller = new AuditoriaController(ServiceProvider);
        var view = new AuditoriaView(controller);
        ShowWindowAsDialog(view, currentWindow);
    }

    private void ShowBackup(Window? currentWindow)
    {
        var controller = new BackupController(ServiceProvider);
        var view = new BackupView(controller);
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
