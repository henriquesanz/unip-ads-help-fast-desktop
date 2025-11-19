using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Presentation.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Linq;

namespace HelpFastDesktop.Presentation.Controllers;

public class DashboardController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private readonly IUsuarioService _usuarioService;
    private readonly NavigationController _navigationController;
    private DashboardModel _model;

    public DashboardController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _chamadoService = serviceProvider.GetRequiredService<IChamadoService>();
        _usuarioService = serviceProvider.GetRequiredService<IUsuarioService>();
        _navigationController = new NavigationController(serviceProvider);
        _model = new DashboardModel();

        ConfigureSections();
        LoadUserInfo();
    }

    public DashboardModel GetModel() => _model;

    private void LoadUserInfo()
    {
        var usuario = _sessionService.UsuarioLogado;
        if (usuario != null)
        {
            _model.NomeUsuario = usuario.Nome;
            _model.TipoUsuario = usuario.TipoUsuarioDisplay;
            _model.DescricaoTipoUsuario = usuario.TipoUsuarioDescription;
        }
    }

    private void ConfigureSections()
    {
        // Limpar todas as seções e ações antes de configurar
        _model.Sections.Clear();
        _model.AllActions.Clear();

        var usuario = _sessionService.UsuarioLogado;
        if (usuario == null) return;

        // Configurar seções baseado no tipo de usuário
        // IMPORTANTE: Apenas um método deve ser chamado por tipo de usuário
        switch (usuario.TipoUsuario)
        {
            case UserRole.Cliente:
                ConfigureClienteSections();
                break;
            case UserRole.Tecnico:
                // Técnicos devem ver apenas o botão de consultar chamados
                ConfigureTecnicoSections();
                break;
            case UserRole.Administrador:
                ConfigureAdministradorSections();
                break;
            default:
                // Caso padrão: não adiciona nenhuma seção
                break;
        }

        UpdateLayoutConfiguration();
    }

    private void ConfigureClienteSections()
    {
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "NOVO CHAMADO",
                    Description = "Abrir novo chamado de suporte",
                    Color = "#0078D7",
                    Command = new RelayCommand(() => NavigateToForm("NovoChamado"))
                },
                new DashboardAction
                {
                    Title = "MEUS CHAMADOS",
                    Description = "Histórico e acompanhamento de chamados",
                    Color = "#00964B",
                    Command = new RelayCommand(() => NavigateToForm("MeusChamados"))
                }
            }
        };

        _model.Sections.Add(chamadosSection);
    }

    private void ConfigureTecnicoSections()
    {
        // Garantir que apenas uma seção seja adicionada para técnicos
        var chamadosSection = new DashboardSection
        {
            Title = "🛠️ Chamados Atribuídos",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "CONSULTAR CHAMADOS",
                    Description = "Visualizar meus chamados atribuídos",
                    Color = "#0078D7",
                    Command = new RelayCommand(() => NavigateToForm("ChamadosAtribuidos"))
                }
            }
        };

        _model.Sections.Add(chamadosSection);
    }

    private void ConfigureAdministradorSections()
    {
        var administracaoSection = new DashboardSection
        {
            Title = "⚙️ Administração do Sistema",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "ATRIBUIR CHAMADOS",
                    Description = "Distribuir chamados entre os técnicos",
                    Color = "#FF5722",
                    Command = new RelayCommand(() => NavigateToForm("AtribuirChamados"))
                },
                new DashboardAction
                {
                    Title = "GERENCIAR USUÁRIOS",
                    Description = "Administrar contas de usuários",
                    Color = "#C85000",
                    Command = new RelayCommand(() => NavigateToForm("GerenciarUsuarios"))
                },
                new DashboardAction
                {
                    Title = "EXTRAIR RELATÓRIOS",
                    Description = "Relatórios e métricas do sistema",
                    Color = "#6432A0",
                    Command = new RelayCommand(() => NavigateToForm("Relatorios"))
                }
            }
        };

        _model.Sections.Add(administracaoSection);
    }

    private void NavigateToForm(string formName)
    {
        OnNavigateToFormRequested?.Invoke(formName);
    }

    private void UpdateLayoutConfiguration()
    {
        _model.AllActions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>(_model.Sections.SelectMany(section => section.Actions));

        var totalActions = _model.AllActions.Count;
        
        int columns;
        if (totalActions <= 5)
        {
            columns = 1;
        }
        else if (totalActions <= 12)
        {
            columns = 2;
        }
        else
        {
            columns = 3;
        }

        _model.ActionColumns = columns;
        _model.ActionsContainerWidth = columns switch
        {
            1 => 320,
            2 => 640,
            _ => 960
        };
    }

    public void Logout()
    {
        _sessionService.FazerLogout();
        OnLogoutRequested?.Invoke();
    }

    public event Action? OnLogoutRequested;
    public event Action<string>? OnNavigateToFormRequested;
}
