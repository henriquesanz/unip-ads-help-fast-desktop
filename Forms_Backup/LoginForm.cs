using HelpFastDesktop.Services;
using HelpFastDesktop.Data;
using HelpFastDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpFastDesktop.Forms;

public partial class LoginForm : Form
{
    private readonly ISessionService _sessionService;

    public LoginForm(ISessionService sessionService)
    {
        _sessionService = sessionService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        // Configurações básicas do formulário
        this.Text = "HELP FAST - Login";
        this.Size = new Size(450, 600);
        this.MinimumSize = new Size(400, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;
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
            Padding = new Padding(20, 20, 20, 20)
        };

        // Logo HELP FAST
        var logoPanel = new Panel
        {
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(0, 120, 215),
            Margin = new Padding(0, 0, 0, 10)
        };

        var logoLabel = new Label
        {
            Text = "HELP FAST",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        logoPanel.Controls.Add(logoLabel);

        // Panel do formulário
        var formPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            Padding = new Padding(40, 30, 40, 30),
            Margin = new Padding(0, 10, 0, 0)
        };

        // Título
        var titleLabel = new Label
        {
            Text = "Faça seu login",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 40,
            Dock = DockStyle.Top
        };

        // Campo Email
        var emailLabel = new Label
        {
            Text = "E-mail:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Height = 25,
            Dock = DockStyle.Top
        };

        var emailTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Height = 35,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Campo Senha
        var senhaLabel = new Label
        {
            Text = "Senha:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            Height = 25,
            Dock = DockStyle.Top
        };

        var senhaTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Height = 35,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            UseSystemPasswordChar = true
        };

        // Botão Login
        var loginButton = new Button
        {
            Text = "ENTRAR",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 120, 215),
            Height = 45,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        loginButton.FlatAppearance.BorderSize = 0;

        // Botão Cadastrar
        var cadastrarButton = new Button
        {
            Text = "CADASTRAR",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(0, 120, 215),
            BackColor = Color.Transparent,
            Height = 35,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        cadastrarButton.FlatAppearance.BorderSize = 0;

        // Label de erro
        var errorLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(255, 100, 100),
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 30,
            Dock = DockStyle.Top
        };

        // Eventos
        loginButton.Click += async (s, e) => await LoginButton_Click(emailTextBox.Text, senhaTextBox.Text, errorLabel);
        cadastrarButton.Click += CadastrarButton_Click;

        // Adicionar controles ao formPanel
        formPanel.Controls.Add(cadastrarButton);
        formPanel.Controls.Add(loginButton);
        formPanel.Controls.Add(errorLabel);
        formPanel.Controls.Add(senhaTextBox);
        formPanel.Controls.Add(senhaLabel);
        formPanel.Controls.Add(emailTextBox);
        formPanel.Controls.Add(emailLabel);
        formPanel.Controls.Add(titleLabel);

        // Adicionar controles ao mainPanel
        mainPanel.Controls.Add(formPanel);
        mainPanel.Controls.Add(logoPanel);

        this.Controls.Add(mainPanel);
    }

    private async Task LoginButton_Click(string email, string senha, Label errorLabel)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            errorLabel.Text = "Por favor, preencha todos os campos.";
            return;
        }

        try
        {
            var loginSucesso = await _sessionService.FazerLoginAsync(email, senha);
            
            if (loginSucesso)
            {
                this.Hide();
                // Para simplificar, vamos criar os serviços manualmente
                // Em uma implementação mais robusta, usaríamos injeção de dependências
                var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase("HelpFastDesktopDB").Options);
                var usuarioService = new UsuarioService(context);
                var chamadoService = new ChamadoService(context, usuarioService);
                var dashboardForm = new DashboardForm(_sessionService, chamadoService, usuarioService);
                dashboardForm.ShowDialog();
                this.Close();
            }
            else
            {
                errorLabel.Text = "E-mail ou senha incorretos.";
            }
        }
        catch (Exception ex)
        {
            errorLabel.Text = $"Erro ao fazer login: {ex.Message}";
        }
    }

    private void CadastrarButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Criar serviços necessários para o cadastro
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("HelpFastDesktopDB").Options);
            var usuarioService = new UsuarioService(context);
            var sessionService = new SessionService(usuarioService);

            var cadastroForm = new CadastroUsuarioForm(sessionService, usuarioService, Models.UserRole.Cliente);
            cadastroForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir cadastro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
