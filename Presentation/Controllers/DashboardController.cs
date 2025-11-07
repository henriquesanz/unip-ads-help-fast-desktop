using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Presentation.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

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
        var usuario = _sessionService.UsuarioLogado;
        if (usuario == null) return;

        switch (usuario.TipoUsuario)
        {
            case UserRole.Cliente:
                ConfigureClienteSections();
                break;
            case UserRole.Tecnico:
                ConfigureTecnicoSections();
                break;
            case UserRole.Administrador:
                ConfigureAdministradorSections();
                break;
        }
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

        var suporteSection = new DashboardSection
        {
            Title = "🔍 Suporte e FAQ",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "CONSULTAR FAQ",
                    Description = "Buscar perguntas frequentes",
                    Color = "#FF8C00",
                    Command = new RelayCommand(() => NavigateToForm("FAQ"))
                },
                new DashboardAction
                {
                    Title = "CHAT COM IA",
                    Description = "Pré-atendimento com inteligência artificial",
                    Color = "#9C27B0",
                    Command = new RelayCommand(() => NavigateToForm("ChatIA"))
                }
            }
        };

        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "NOTIFICAÇÕES",
                    Description = "Visualizar notificações recebidas",
                    Color = "#E91E63",
                    Command = new RelayCommand(() => NavigateToForm("Notificacoes"))
                }
            }
        };

        _model.Sections.Add(chamadosSection);
        _model.Sections.Add(suporteSection);
        _model.Sections.Add(notificacoesSection);
    }

    private void ConfigureTecnicoSections()
    {
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "CHAMADOS ATRIBUÍDOS",
                    Description = "Visualizar meus chamados atribuídos",
                    Color = "#0078D7",
                    Command = new RelayCommand(() => NavigateToForm("ChamadosAtribuidos"))
                },
                new DashboardAction
                {
                    Title = "TODOS OS CHAMADOS",
                    Description = "Visualizar todos os chamados do sistema",
                    Color = "#00964B",
                    Command = new RelayCommand(() => NavigateToForm("TodosChamados"))
                }
            }
        };

        var relatoriosSection = new DashboardSection
        {
            Title = "📊 Relatórios e Performance",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "RELATÓRIOS",
                    Description = "Relatórios de performance e métricas",
                    Color = "#6432A0",
                    Command = new RelayCommand(() => NavigateToForm("Relatorios"))
                }
            }
        };

        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "NOTIFICAÇÕES",
                    Description = "Visualizar notificações do técnico",
                    Color = "#E91E63",
                    Command = new RelayCommand(() => NavigateToForm("Notificacoes"))
                }
            }
        };

        _model.Sections.Add(chamadosSection);
        _model.Sections.Add(relatoriosSection);
        _model.Sections.Add(notificacoesSection);
    }

    private void ConfigureAdministradorSections()
    {
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "TODOS OS CHAMADOS",
                    Description = "Gerenciar todos os chamados do sistema",
                    Color = "#0078D7",
                    Command = new RelayCommand(() => NavigateToForm("TodosChamados"))
                },
                new DashboardAction
                {
                    Title = "CHAMADOS ATRIBUÍDOS",
                    Description = "Visualizar chamados atribuídos",
                    Color = "#00964B",
                    Command = new RelayCommand(() => NavigateToForm("ChamadosAtribuidos"))
                },
                new DashboardAction
                {
                    Title = "ATRIBUIR CHAMADOS",
                    Description = "Atribuir chamados para técnicos",
                    Color = "#FF5722",
                    Command = new RelayCommand(() => NavigateToForm("AtribuirChamados"))
                }
            }
        };

        var usuariosSection = new DashboardSection
        {
            Title = "👥 Gestão de Usuários",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "GERENCIAR USUÁRIOS",
                    Description = "Gerenciar usuários do sistema",
                    Color = "#C85000",
                    Command = new RelayCommand(() => NavigateToForm("GerenciarUsuarios"))
                },
                new DashboardAction
                {
                    Title = "CADASTRAR USUÁRIO",
                    Description = "Criar novos usuários",
                    Color = "#4CAF50",
                    Command = new RelayCommand(() => NavigateToForm("CadastrarUsuario"))
                },
                new DashboardAction
                {
                    Title = "ALTERAR PERMISSÕES",
                    Description = "Configurar permissões e hierarquia",
                    Color = "#9C27B0",
                    Command = new RelayCommand(() => NavigateToForm("Permissoes"))
                }
            }
        };

        var relatoriosSection = new DashboardSection
        {
            Title = "📊 Relatórios e Análises",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "RELATÓRIOS EXECUTIVOS",
                    Description = "Relatórios e métricas do sistema",
                    Color = "#6432A0",
                    Command = new RelayCommand(() => NavigateToForm("Relatorios"))
                },
                new DashboardAction
                {
                    Title = "MÉTRICAS DE PERFORMANCE",
                    Description = "Análise de performance por técnico",
                    Color = "#FF9800",
                    Command = new RelayCommand(() => NavigateToForm("Metricas"))
                },
                new DashboardAction
                {
                    Title = "ANÁLISE DE SATISFAÇÃO",
                    Description = "Relatórios de satisfação do cliente",
                    Color = "#2196F3",
                    Command = new RelayCommand(() => NavigateToForm("Satisfacao"))
                }
            }
        };

        var configuracoesSection = new DashboardSection
        {
            Title = "⚙️ Configurações do Sistema",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "CONFIGURAÇÕES",
                    Description = "Configurações gerais do sistema",
                    Color = "#607D8B",
                    Command = new RelayCommand(() => NavigateToForm("Configuracoes"))
                },
                new DashboardAction
                {
                    Title = "LOGS DE AUDITORIA",
                    Description = "Visualizar logs de auditoria",
                    Color = "#795548",
                    Command = new RelayCommand(() => NavigateToForm("Auditoria"))
                },
                new DashboardAction
                {
                    Title = "BACKUP E RESTAURAÇÃO",
                    Description = "Gerenciar backup do sistema",
                    Color = "#9E9E9E",
                    Command = new RelayCommand(() => NavigateToForm("Backup"))
                }
            }
        };

        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações e Comunicação",
            Actions = new System.Collections.ObjectModel.ObservableCollection<DashboardAction>
            {
                new DashboardAction
                {
                    Title = "NOTIFICAÇÕES",
                    Description = "Visualizar notificações do sistema",
                    Color = "#E91E63",
                    Command = new RelayCommand(() => NavigateToForm("Notificacoes"))
                },
                new DashboardAction
                {
                    Title = "CONFIGURAR NOTIFICAÇÕES",
                    Description = "Configurar tipos de notificação",
                    Color = "#F44336",
                    Command = new RelayCommand(() => NavigateToForm("ConfigNotificacoes"))
                }
            }
        };

        _model.Sections.Add(chamadosSection);
        _model.Sections.Add(usuariosSection);
        _model.Sections.Add(relatoriosSection);
        _model.Sections.Add(configuracoesSection);
        _model.Sections.Add(notificacoesSection);
    }

    private void NavigateToForm(string formName)
    {
        OnNavigateToFormRequested?.Invoke(formName);
    }

    public void Logout()
    {
        _sessionService.FazerLogout();
        OnLogoutRequested?.Invoke();
    }

    public event Action? OnLogoutRequested;
    public event Action<string>? OnNavigateToFormRequested;
}
