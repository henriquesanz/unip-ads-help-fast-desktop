using HelpFastDesktop.Presentation.Commands;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HelpFastDesktop.Presentation.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private readonly IUsuarioService _usuarioService;

    private string _nomeUsuario = string.Empty;
    private string _tipoUsuario = string.Empty;
    private string _descricaoTipoUsuario = string.Empty;

    public DashboardViewModel(ISessionService sessionService, IChamadoService chamadoService, IUsuarioService usuarioService)
    {
        _sessionService = sessionService;
        _chamadoService = chamadoService;
        _usuarioService = usuarioService;

        LogoutCommand = new RelayCommand(() => Logout());
        
        // Configurar seções baseadas no tipo de usuário
        ConfigureSections();
        LoadUserInfo();
    }

    public string NomeUsuario
    {
        get => _nomeUsuario;
        set => SetProperty(ref _nomeUsuario, value);
    }

    public string TipoUsuario
    {
        get => _tipoUsuario;
        set => SetProperty(ref _tipoUsuario, value);
    }

    public string DescricaoTipoUsuario
    {
        get => _descricaoTipoUsuario;
        set => SetProperty(ref _descricaoTipoUsuario, value);
    }

    public ObservableCollection<DashboardSection> Sections { get; } = new();

    public ICommand LogoutCommand { get; }

    public event Action? LogoutRequested;
    public event Action<string>? NavigateToFormRequested;

    private void LoadUserInfo()
    {
        var usuario = _sessionService.UsuarioLogado;
        if (usuario != null)
        {
            NomeUsuario = usuario.Nome;
            TipoUsuario = usuario.TipoUsuarioDisplay;
            DescricaoTipoUsuario = usuario.TipoUsuarioDescription;
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
        // Seção de Chamados
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Suporte e FAQ
        var suporteSection = new DashboardSection
        {
            Title = "🔍 Suporte e FAQ",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Notificações
        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações",
            Actions = new ObservableCollection<DashboardAction>
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

        Sections.Add(chamadosSection);
        Sections.Add(suporteSection);
        Sections.Add(notificacoesSection);
    }

    private void ConfigureTecnicoSections()
    {
        // Seção de Chamados
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Relatórios
        var relatoriosSection = new DashboardSection
        {
            Title = "📊 Relatórios e Performance",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Notificações
        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações",
            Actions = new ObservableCollection<DashboardAction>
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

        Sections.Add(chamadosSection);
        Sections.Add(relatoriosSection);
        Sections.Add(notificacoesSection);
    }

    private void ConfigureAdministradorSections()
    {
        // Seção de Gestão de Chamados
        var chamadosSection = new DashboardSection
        {
            Title = "📋 Gestão de Chamados",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Gestão de Usuários
        var usuariosSection = new DashboardSection
        {
            Title = "👥 Gestão de Usuários",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Relatórios e Análises
        var relatoriosSection = new DashboardSection
        {
            Title = "📊 Relatórios e Análises",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Configurações do Sistema
        var configuracoesSection = new DashboardSection
        {
            Title = "⚙️ Configurações do Sistema",
            Actions = new ObservableCollection<DashboardAction>
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

        // Seção de Notificações
        var notificacoesSection = new DashboardSection
        {
            Title = "🔔 Notificações e Comunicação",
            Actions = new ObservableCollection<DashboardAction>
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

        Sections.Add(chamadosSection);
        Sections.Add(usuariosSection);
        Sections.Add(relatoriosSection);
        Sections.Add(configuracoesSection);
        Sections.Add(notificacoesSection);
    }

    private void NavigateToForm(string formName)
    {
        NavigateToFormRequested?.Invoke(formName);
    }

    private void Logout()
    {
        LogoutRequested?.Invoke();
    }
}

public class DashboardSection
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<DashboardAction> Actions { get; set; } = new();
}

public class DashboardAction
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public ICommand Command { get; set; } = new RelayCommand(() => { });
}
