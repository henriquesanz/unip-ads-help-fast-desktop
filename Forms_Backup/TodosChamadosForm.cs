using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class TodosChamadosForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;

    public TodosChamadosForm(ISessionService sessionService, IChamadoService chamadoService)
    {
        _sessionService = sessionService;
        _chamadoService = chamadoService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = "HELP FAST - Todos os Chamados";
        this.Size = new Size(1100, 700);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.BackColor = Color.FromArgb(45, 45, 48);

        SetupControls();
        LoadChamados();
    }

    private void SetupControls()
    {
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(20)
        };

        // Título
        var titleLabel = new Label
        {
            Text = "Todos os Chamados",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 40,
            Dock = DockStyle.Top
        };

        // ListView para exibir chamados
        var chamadosListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.FromArgb(60, 60, 63),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9)
        };

        chamadosListView.Columns.Add("ID", 50);
        chamadosListView.Columns.Add("Título", 200);
        chamadosListView.Columns.Add("Cliente", 150);
        chamadosListView.Columns.Add("Status", 100);
        chamadosListView.Columns.Add("Prioridade", 100);
        chamadosListView.Columns.Add("Data Criação", 120);

        mainPanel.Controls.Add(chamadosListView);
        mainPanel.Controls.Add(titleLabel);

        this.Controls.Add(mainPanel);
    }

    private async void LoadChamados()
    {
        try
        {
            var chamados = await _chamadoService.ListarTodosChamadosAsync();
            
            var listView = this.Controls[0].Controls[0] as ListView;
            if (listView != null)
            {
                listView.Items.Clear();
                foreach (var chamado in chamados)
                {
                    var item = new ListViewItem(chamado.Id.ToString());
                    item.SubItems.Add(chamado.Titulo);
                    item.SubItems.Add(chamado.Usuario?.Nome ?? "N/A");
                    item.SubItems.Add(chamado.StatusDisplay);
                    item.SubItems.Add(chamado.PrioridadeDisplay);
                    item.SubItems.Add(chamado.DataCriacao.ToString("dd/MM/yyyy HH:mm"));
                    listView.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar chamados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

