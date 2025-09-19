using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class RelatoriosForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;

    public RelatoriosForm(ISessionService sessionService, IChamadoService chamadoService)
    {
        _sessionService = sessionService;
        _chamadoService = chamadoService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = "HELP FAST - Relatórios";
        this.Size = new Size(900, 700);
        this.MinimumSize = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.BackColor = Color.FromArgb(45, 45, 48);

        SetupControls();
    }

    private void SetupControls()
    {
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(30)
        };

        // Título
        var titleLabel = new Label
        {
            Text = "Relatórios do Sistema",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 40,
            Dock = DockStyle.Top
        };

        // Mensagem informativa
        var infoLabel = new Label
        {
            Text = "Funcionalidade de relatórios será implementada em breve.\n\nAqui você poderá visualizar:\n• Estatísticas de chamados\n• Relatórios de performance\n• Análises de tendências\n• Exportação de dados",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(200, 200, 200),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        mainPanel.Controls.Add(infoLabel);
        mainPanel.Controls.Add(titleLabel);

        this.Controls.Add(mainPanel);
    }
}

