using HelpFastDesktop.Models;
using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class NovoChamadoForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IChamadoService _chamadoService;

    public NovoChamadoForm(ISessionService sessionService, IChamadoService chamadoService)
    {
        _sessionService = sessionService;
        _chamadoService = chamadoService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = "HELP FAST - Novo Chamado";
        this.Size = new Size(700, 600);
        this.MinimumSize = new Size(600, 500);
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
            Text = "Solicitar Novo Chamado",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 40,
            Dock = DockStyle.Top
        };

        // Campo Título
        var tituloLabel = new Label
        {
            Text = "Título do Chamado:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Height = 25,
            Dock = DockStyle.Top
        };

        var tituloTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Height = 35,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Campo Descrição
        var descricaoLabel = new Label
        {
            Text = "Descrição:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Height = 25,
            Dock = DockStyle.Top
        };

        var descricaoTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        // Campo Prioridade
        var prioridadeLabel = new Label
        {
            Text = "Prioridade:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Height = 25,
            Dock = DockStyle.Top
        };

        var prioridadeComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Height = 35,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        prioridadeComboBox.Items.AddRange(new[] { "Baixa", "Média", "Alta", "Crítica" });
        prioridadeComboBox.SelectedIndex = 1; // Média por padrão

        // Botões
        var buttonsPanel = new Panel
        {
            Height = 50,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(45, 45, 48)
        };

        var criarButton = new Button
        {
            Text = "CRIAR CHAMADO",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 120, 215),
            Size = new Size(150, 40),
            Location = new Point(300, 5),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        var cancelarButton = new Button
        {
            Text = "CANCELAR",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(100, 100, 100),
            Size = new Size(120, 40),
            Location = new Point(460, 5),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        criarButton.FlatAppearance.BorderSize = 0;
        cancelarButton.FlatAppearance.BorderSize = 0;

        criarButton.Click += async (s, e) => await CriarChamado_Click(tituloTextBox.Text, descricaoTextBox.Text, prioridadeComboBox.Text);
        cancelarButton.Click += (s, e) => this.Close();

        buttonsPanel.Controls.Add(cancelarButton);
        buttonsPanel.Controls.Add(criarButton);

        mainPanel.Controls.Add(buttonsPanel);
        mainPanel.Controls.Add(prioridadeComboBox);
        mainPanel.Controls.Add(prioridadeLabel);
        mainPanel.Controls.Add(descricaoTextBox);
        mainPanel.Controls.Add(descricaoLabel);
        mainPanel.Controls.Add(tituloTextBox);
        mainPanel.Controls.Add(tituloLabel);
        mainPanel.Controls.Add(titleLabel);

        this.Controls.Add(mainPanel);
    }

    private async Task CriarChamado_Click(string titulo, string descricao, string prioridade)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
        {
            MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var chamado = new Chamado
            {
                Titulo = titulo,
                Descricao = descricao,
                Prioridade = prioridade,
                Status = "Aberto",
                UsuarioId = _sessionService.UsuarioLogado!.Id,
                DataCriacao = DateTime.Now
            };

            await _chamadoService.CriarChamadoAsync(chamado);

            MessageBox.Show("Chamado criado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao criar chamado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

