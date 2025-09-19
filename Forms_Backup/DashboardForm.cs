using HelpFastDesktop.Models;
using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class DashboardForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;
    private readonly IUsuarioService _usuarioService;

    public DashboardForm(ISessionService sessionService, IChamadoService chamadoService, IUsuarioService usuarioService)
    {
        _sessionService = sessionService;
        _chamadoService = chamadoService;
        _usuarioService = usuarioService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        // Configurações básicas do formulário
        this.Text = "HELP FAST - Dashboard";
        this.Size = new Size(1200, 800);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.BackColor = Color.FromArgb(45, 45, 48);

        // Configurar controles
        SetupControls();
    }

    private void SetupControls()
    {
        // Panel principal
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(20)
        };

        // Header com informações do usuário
        SetupHeader(mainPanel);

        // Panel de conteúdo principal
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            Padding = new Padding(20)
        };

        // Título principal
        var mainTitle = new Label
        {
            Text = "Dashboard - Ações Disponíveis",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 50,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Panel de conteúdo com scroll
        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            AutoScroll = true,
            Padding = new Padding(10)
        };

        SetupDashboardContent(scrollPanel);

        contentPanel.Controls.Add(scrollPanel);
        contentPanel.Controls.Add(mainTitle);
        mainPanel.Controls.Add(contentPanel);

        this.Controls.Add(mainPanel);
    }

    private void SetupHeader(Panel mainPanel)
    {
        var headerPanel = new Panel
        {
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(0, 120, 215),
            Padding = new Padding(20, 10, 20, 10)
        };

        // Layout do header com TableLayoutPanel
        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.FromArgb(0, 120, 215)
        };

        // Configurar colunas
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Informações do usuário
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Espaço vazio
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Botão logout

        // Informações do usuário
        var userInfoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        var welcomeLabel = new Label
        {
            Text = $"Bem-vindo, {_sessionService.UsuarioLogado?.Nome ?? "Usuário"}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 35,
            Dock = DockStyle.Top
        };

        var userTypeLabel = new Label
        {
            Text = $"{_sessionService.UsuarioLogado?.TipoUsuarioDisplay} - {_sessionService.UsuarioLogado?.TipoUsuarioDescription}",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(220, 220, 220),
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 25,
            Dock = DockStyle.Top
        };

        userInfoPanel.Controls.Add(userTypeLabel);
        userInfoPanel.Controls.Add(welcomeLabel);

        // Botão logout
        var logoutButton = CreateHeaderButton("LOGOUT", Color.FromArgb(200, 50, 50));
        logoutButton.Click += LogoutButton_Click;

        headerLayout.Controls.Add(userInfoPanel, 0, 0);
        headerLayout.Controls.Add(logoutButton, 2, 0);

        headerPanel.Controls.Add(headerLayout);
        mainPanel.Controls.Add(headerPanel);
    }

    private Button CreateHeaderButton(string text, Color backColor)
    {
        var button = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = backColor,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(5)
        };

        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(Math.Min(255, backColor.R + 20), Math.Min(255, backColor.G + 20), Math.Min(255, backColor.B + 20));
        button.MouseLeave += (s, e) => button.BackColor = backColor;

        return button;
    }

    private void SetupDashboardContent(Panel contentPanel)
    {
        var usuario = _sessionService.UsuarioLogado;
        if (usuario == null) return;

        // Layout principal com TableLayoutPanel
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            ColumnCount = 1,
            RowCount = 0,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10)
        };

        // Configurar coluna
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // Adicionar seções baseadas no tipo de usuário
        switch (usuario.TipoUsuario)
        {
            case UserRole.Cliente:
                AddClienteSections(mainLayout);
                break;
            case UserRole.Tecnico:
                AddTecnicoSections(mainLayout);
                break;
            case UserRole.Administrador:
                AddAdministradorSections(mainLayout);
                break;
        }

        contentPanel.Controls.Add(mainLayout);
    }

    private void AddClienteSections(TableLayoutPanel mainLayout)
    {
        // Seção de Chamados
        AddSection(mainLayout, "📋 Gestão de Chamados", new List<(string text, string description, Color color, Action action)>
        {
            ("NOVO CHAMADO", "Solicitar novo chamado de suporte", Color.FromArgb(0, 120, 215), () => OpenForm<NovoChamadoForm>()),
            ("MEUS CHAMADOS", "Visualizar e acompanhar meus chamados", Color.FromArgb(0, 150, 100), () => OpenForm<MeusChamadosForm>())
        });
    }

    private void AddTecnicoSections(TableLayoutPanel mainLayout)
    {
        // Seção de Chamados
        AddSection(mainLayout, "📋 Gestão de Chamados", new List<(string text, string description, Color color, Action action)>
        {
            ("TODOS OS CHAMADOS", "Visualizar todos os chamados do sistema", Color.FromArgb(0, 120, 215), () => OpenForm<TodosChamadosForm>()),
            ("CHAMADOS ATRIBUÍDOS", "Visualizar meus chamados atribuídos", Color.FromArgb(0, 150, 100), () => OpenForm<ChamadosAtribuidosForm>())
        });
    }

    private void AddAdministradorSections(TableLayoutPanel mainLayout)
    {
        // Seção de Gestão de Usuários
        AddSection(mainLayout, "👥 Gestão de Usuários", new List<(string text, string description, Color color, Action action)>
        {
            ("GERENCIAR USUÁRIOS", "Gerenciar usuários do sistema", Color.FromArgb(200, 100, 0), () => OpenForm<GerenciarUsuariosForm>())
        });

        // Seção de Chamados
        AddSection(mainLayout, "📋 Gestão de Chamados", new List<(string text, string description, Color color, Action action)>
        {
            ("TODOS OS CHAMADOS", "Visualizar todos os chamados do sistema", Color.FromArgb(0, 120, 215), () => OpenForm<TodosChamadosForm>()),
            ("CHAMADOS ATRIBUÍDOS", "Visualizar chamados atribuídos", Color.FromArgb(0, 150, 100), () => OpenForm<ChamadosAtribuidosForm>())
        });

        // Seção de Relatórios
        AddSection(mainLayout, "📊 Relatórios e Análises", new List<(string text, string description, Color color, Action action)>
        {
            ("RELATÓRIOS", "Visualizar relatórios do sistema", Color.FromArgb(100, 50, 150), () => OpenForm<RelatoriosForm>())
        });
    }

    private void AddSection(TableLayoutPanel mainLayout, string sectionTitle, List<(string text, string description, Color color, Action action)> buttons)
    {
        // Adicionar espaçamento
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

        // Título da seção
        var titleLabel = new Label
        {
            Text = sectionTitle,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 120, 215),
            Height = 35,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 5, 0, 0)
        };

        mainLayout.Controls.Add(titleLabel, 0, mainLayout.RowCount - 1);

        // Adicionar linha vazia
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));

        // Panel de botões da seção
        var sectionPanel = new Panel
        {
            Height = 120,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(70, 70, 73),
            Padding = new Padding(15),
            Margin = new Padding(10, 0, 10, 10)
        };

        // Layout dos botões na seção
        var buttonsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = Math.Min(buttons.Count, 3), // Máximo 3 colunas
            RowCount = (int)Math.Ceiling((double)buttons.Count / Math.Min(buttons.Count, 3)),
            BackColor = Color.FromArgb(70, 70, 73)
        };

        // Configurar colunas
        for (int i = 0; i < buttonsLayout.ColumnCount; i++)
        {
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / buttonsLayout.ColumnCount));
        }

        // Configurar linhas
        for (int i = 0; i < buttonsLayout.RowCount; i++)
        {
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        }

        // Adicionar botões
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = CreateCardButton(buttons[i].text, buttons[i].description, buttons[i].color);
            button.Click += (s, e) => buttons[i].action();

            int row = i / buttonsLayout.ColumnCount;
            int col = i % buttonsLayout.ColumnCount;
            buttonsLayout.Controls.Add(button, col, row);
        }

        sectionPanel.Controls.Add(buttonsLayout);
        mainLayout.Controls.Add(sectionPanel, 0, mainLayout.RowCount - 1);
    }

    private Button CreateCardButton(string text, string description, Color backColor)
    {
        var button = new Button
        {
            Text = $"{text}\n\n{description}",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = backColor,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(8),
            TextAlign = ContentAlignment.MiddleCenter,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
        
        // Efeitos hover
        button.MouseEnter += (s, e) => 
        {
            button.BackColor = Color.FromArgb(
                Math.Min(255, backColor.R + 30), 
                Math.Min(255, backColor.G + 30), 
                Math.Min(255, backColor.B + 30)
            );
            button.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        };
        
        button.MouseLeave += (s, e) => 
        {
            button.BackColor = backColor;
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        };

        return button;
    }

    private void OpenForm<T>() where T : Form
    {
        try
        {
            // Para simplificar, vamos criar os formulários manualmente
            // Em uma implementação mais robusta, usaríamos injeção de dependências
            Form form = typeof(T).Name switch
            {
                nameof(NovoChamadoForm) => new NovoChamadoForm(_sessionService, _chamadoService),
                nameof(MeusChamadosForm) => new MeusChamadosForm(_sessionService, _chamadoService),
                nameof(TodosChamadosForm) => new TodosChamadosForm(_sessionService, _chamadoService),
                nameof(ChamadosAtribuidosForm) => new ChamadosAtribuidosForm(_sessionService, _chamadoService),
                nameof(GerenciarUsuariosForm) => new GerenciarUsuariosForm(_sessionService, _usuarioService),
                nameof(RelatoriosForm) => new RelatoriosForm(_sessionService, _chamadoService),
                _ => throw new NotSupportedException($"Formulário {typeof(T).Name} não suportado")
            };
            
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir formulário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Deseja realmente sair do sistema?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _sessionService.FazerLogout();
            this.Close();
        }
    }
}
