using HelpFastDesktop.Models;
using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class CadastroUsuarioForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IUsuarioService _usuarioService;
    private readonly UserRole _tipoUsuario;
    private readonly Usuario? _usuarioParaEditar;

    // Controles
    private TextBox? _nomeTextBox;
    private TextBox? _emailTextBox;
    private TextBox? _telefoneTextBox;
    private TextBox? _senhaTextBox;
    private ComboBox? _tipoUsuarioComboBox;
    private Button? _salvarButton;
    private Button? _cancelarButton;
    private Label? _errorLabel;

    public CadastroUsuarioForm(ISessionService sessionService, IUsuarioService usuarioService, 
        UserRole tipoUsuario = UserRole.Cliente, Usuario? usuarioParaEditar = null)
    {
        _sessionService = sessionService;
        _usuarioService = usuarioService;
        _tipoUsuario = tipoUsuario;
        _usuarioParaEditar = usuarioParaEditar;
        
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = _usuarioParaEditar != null ? "HELP FAST - Editar Usuário" : "HELP FAST - Cadastro de Usuário";
        this.Size = new Size(500, 600);
        this.MinimumSize = new Size(450, 550);
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
            Text = _usuarioParaEditar != null ? "Editar Usuário" : "Cadastro de Usuário",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 40,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Panel do formulário
        var formPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            Padding = new Padding(20)
        };

        SetupFormFields(formPanel);

        // Panel de botões
        var buttonsPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(60, 60, 63),
            Padding = new Padding(20, 10, 20, 20)
        };

        SetupButtons(buttonsPanel);

        mainPanel.Controls.Add(buttonsPanel);
        mainPanel.Controls.Add(formPanel);
        mainPanel.Controls.Add(titleLabel);

        this.Controls.Add(mainPanel);
    }

    private void SetupFormFields(Panel formPanel)
    {
        // Usar TableLayoutPanel para layout responsivo
        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(60, 60, 63),
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 0,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        // Configurar coluna
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // Adicionar espaçamento entre linhas
        var rowHeight = 60;

        // Campo Nome
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        AddFormField(tableLayout, "Nome Completo:", out _nomeTextBox);

        // Campo Email
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        AddFormField(tableLayout, "E-mail:", out _emailTextBox);
        _emailTextBox!.TextChanged += EmailTextBox_TextChanged;

        // Campo Telefone
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        AddFormField(tableLayout, "Telefone:", out _telefoneTextBox);
        _telefoneTextBox!.KeyPress += TelefoneTextBox_KeyPress;

        // Campo Senha
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        var senhaLabelText = _usuarioParaEditar != null ? "Nova Senha (deixe em branco para manter):" : "Senha:";
        AddFormField(tableLayout, senhaLabelText, out _senhaTextBox);
        _senhaTextBox!.UseSystemPasswordChar = true;

        // Campo Tipo de Usuário (apenas para administradores)
        if (_sessionService.UsuarioLogado?.TipoUsuario == UserRole.Administrador)
        {
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
            
            var tipoLabel = CreateResponsiveLabel("Tipo de Usuário:");
            tableLayout.Controls.Add(tipoLabel, 0, tableLayout.RowCount - 1);

            _tipoUsuarioComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(80, 80, 83),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 0, 0)
            };

            // Adicionar opções baseadas no tipo de usuário
            if (_tipoUsuario == UserRole.Cliente)
            {
                _tipoUsuarioComboBox.Items.Add("Cliente");
            }
            else if (_tipoUsuario == UserRole.Tecnico)
            {
                _tipoUsuarioComboBox.Items.Add("Cliente");
                _tipoUsuarioComboBox.Items.Add("Técnico");
            }
            else if (_tipoUsuario == UserRole.Administrador)
            {
                _tipoUsuarioComboBox.Items.Add("Cliente");
                _tipoUsuarioComboBox.Items.Add("Técnico");
                _tipoUsuarioComboBox.Items.Add("Administrador");
            }

            _tipoUsuarioComboBox.SelectedIndex = 0;
            tableLayout.Controls.Add(_tipoUsuarioComboBox, 0, tableLayout.RowCount - 1);
            tableLayout.SetRow(_tipoUsuarioComboBox, tableLayout.RowCount - 1);
        }

        // Label de erro
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        _errorLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(255, 100, 100),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false,
            Margin = new Padding(0, 10, 0, 0)
        };
        tableLayout.Controls.Add(_errorLabel, 0, tableLayout.RowCount - 1);

        formPanel.Controls.Add(tableLayout);

        // Preencher campos se estiver editando
        if (_usuarioParaEditar != null)
        {
            _nomeTextBox!.Text = _usuarioParaEditar.Nome;
            _emailTextBox!.Text = _usuarioParaEditar.Email;
            _telefoneTextBox!.Text = _usuarioParaEditar.Telefone;
            _senhaTextBox!.Text = ""; // Senha em branco para manter a atual
            
            if (_tipoUsuarioComboBox != null)
            {
                _tipoUsuarioComboBox.SelectedItem = _usuarioParaEditar.TipoUsuarioDisplay;
            }
        }
    }

    private void AddFormField(TableLayoutPanel tableLayout, string labelText, out TextBox? textBox)
    {
        var label = CreateResponsiveLabel(labelText);
        textBox = CreateResponsiveTextBox();
        
        tableLayout.Controls.Add(label, 0, tableLayout.RowCount - 1);
        tableLayout.Controls.Add(textBox, 0, tableLayout.RowCount - 1);
        tableLayout.SetRow(textBox, tableLayout.RowCount - 1);
    }

    private Label CreateResponsiveLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 5)
        };
    }

    private TextBox CreateResponsiveTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(80, 80, 83),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 5, 0, 0)
        };
    }


    private void SetupButtons(Panel buttonsPanel)
    {
        // Usar TableLayoutPanel para botões responsivos
        var buttonLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.FromArgb(60, 60, 63)
        };

        // Configurar colunas
        buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Espaço vazio
        buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Botão Salvar
        buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Botão Cancelar

        // Configurar linha
        buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _salvarButton = new Button
        {
            Text = _usuarioParaEditar != null ? "ATUALIZAR" : "CADASTRAR",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 120, 215),
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(5)
        };
        _salvarButton.FlatAppearance.BorderSize = 0;
        _salvarButton.Click += SalvarButton_Click;

        _cancelarButton = new Button
        {
            Text = "CANCELAR",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(200, 200, 200),
            BackColor = Color.FromArgb(80, 80, 83),
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(5)
        };
        _cancelarButton.FlatAppearance.BorderSize = 0;
        _cancelarButton.Click += (s, e) => this.Close();

        buttonLayout.Controls.Add(_salvarButton, 1, 0);
        buttonLayout.Controls.Add(_cancelarButton, 2, 0);

        buttonsPanel.Controls.Add(buttonLayout);
    }

    private void EmailTextBox_TextChanged(object? sender, EventArgs e)
    {
        if (_errorLabel != null)
        {
            _errorLabel.Visible = false;
        }
    }

    private void TelefoneTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Permitir apenas números, parênteses, espaços, hífen e backspace
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && 
            e.KeyChar != '(' && e.KeyChar != ')' && e.KeyChar != ' ' && e.KeyChar != '-')
        {
            e.Handled = true;
        }
    }

    private async void SalvarButton_Click(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
            return;

        try
        {
            _salvarButton!.Enabled = false;
            _salvarButton.Text = "SALVANDO...";

            if (_usuarioParaEditar != null)
            {
                await AtualizarUsuarioAsync();
            }
            else
            {
                await CriarUsuarioAsync();
            }

            MessageBox.Show(
                _usuarioParaEditar != null ? "Usuário atualizado com sucesso!" : "Usuário cadastrado com sucesso!",
                "Sucesso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Erro ao salvar usuário: {ex.Message}");
        }
        finally
        {
            _salvarButton!.Enabled = true;
            _salvarButton.Text = _usuarioParaEditar != null ? "ATUALIZAR" : "CADASTRAR";
        }
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(_nomeTextBox?.Text))
        {
            ShowError("Nome é obrigatório.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_emailTextBox?.Text))
        {
            ShowError("E-mail é obrigatório.");
            return false;
        }

        if (!IsValidEmail(_emailTextBox.Text))
        {
            ShowError("E-mail inválido.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_telefoneTextBox?.Text))
        {
            ShowError("Telefone é obrigatório.");
            return false;
        }

        if (_usuarioParaEditar == null && string.IsNullOrWhiteSpace(_senhaTextBox?.Text))
        {
            ShowError("Senha é obrigatória para novos usuários.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_senhaTextBox?.Text) && _senhaTextBox.Text.Length < 6)
        {
            ShowError("Senha deve ter pelo menos 6 caracteres.");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message)
    {
        if (_errorLabel != null)
        {
            _errorLabel.Text = message;
            _errorLabel.Visible = true;
        }
    }

    private async Task CriarUsuarioAsync()
    {
        var usuario = new Usuario
        {
            Nome = _nomeTextBox!.Text.Trim(),
            Email = _emailTextBox!.Text.Trim().ToLower(),
            Telefone = _telefoneTextBox!.Text.Trim(),
            Senha = _senhaTextBox!.Text,
            DataCriacao = DateTime.Now,
            Ativo = true
        };

        // Verificar se e-mail já existe
        if (await _usuarioService.EmailExisteAsync(usuario.Email))
        {
            throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
        }

        // Verificar permissões para criar usuário
        var usuarioLogado = _sessionService.UsuarioLogado;

        // Determinar tipo de usuário
        UserRole tipoUsuario;
        if (_tipoUsuarioComboBox != null)
        {
            tipoUsuario = _tipoUsuarioComboBox.SelectedItem?.ToString() switch
            {
                "Cliente" => UserRole.Cliente,
                "Técnico" => UserRole.Tecnico,
                "Administrador" => UserRole.Administrador,
                _ => UserRole.Cliente
            };
        }
        else
        {
            tipoUsuario = _tipoUsuario;
        }

        // Verificar permissões (apenas se usuário estiver logado)
        if (usuarioLogado != null && !await _usuarioService.PodeCriarUsuarioAsync(usuarioLogado.Id, tipoUsuario))
        {
            throw new UnauthorizedAccessException("Você não tem permissão para criar usuários deste tipo.");
        }

        // Criar usuário baseado no tipo
        switch (tipoUsuario)
        {
            case UserRole.Cliente:
                await _usuarioService.CriarClienteAsync(usuario);
                break;
            case UserRole.Tecnico:
                if (usuarioLogado == null)
                    throw new UnauthorizedAccessException("Usuário não autenticado.");
                await _usuarioService.CriarTecnicoAsync(usuario, usuarioLogado.Id);
                break;
            case UserRole.Administrador:
                if (usuarioLogado == null)
                    throw new UnauthorizedAccessException("Usuário não autenticado.");
                await _usuarioService.CriarAdministradorAsync(usuario, usuarioLogado.Id);
                break;
        }
    }

    private async Task AtualizarUsuarioAsync()
    {
        if (_usuarioParaEditar == null) return;

        // Verificar se e-mail mudou e se já existe
        if (_usuarioParaEditar.Email != _emailTextBox!.Text.Trim().ToLower())
        {
            if (await _usuarioService.EmailExisteAsync(_emailTextBox.Text.Trim().ToLower()))
            {
                throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
            }
        }

        // Atualizar dados
        _usuarioParaEditar.Nome = _nomeTextBox!.Text.Trim();
        _usuarioParaEditar.Email = _emailTextBox.Text.Trim().ToLower();
        _usuarioParaEditar.Telefone = _telefoneTextBox!.Text.Trim();

        // Atualizar senha se fornecida
        if (!string.IsNullOrWhiteSpace(_senhaTextBox!.Text))
        {
            _usuarioParaEditar.Senha = _senhaTextBox.Text;
        }

        // Atualizar tipo se permitido
        if (_tipoUsuarioComboBox != null && _sessionService.UsuarioLogado?.TipoUsuario == UserRole.Administrador)
        {
            var novoTipo = _tipoUsuarioComboBox.SelectedItem?.ToString() switch
            {
                "Cliente" => UserRole.Cliente,
                "Técnico" => UserRole.Tecnico,
                "Administrador" => UserRole.Administrador,
                _ => _usuarioParaEditar.TipoUsuario
            };

            if (novoTipo != _usuarioParaEditar.TipoUsuario)
            {
                // Verificar permissões para alterar tipo
                var usuarioLogado = _sessionService.UsuarioLogado;
                if (usuarioLogado != null && await _usuarioService.PodeCriarUsuarioAsync(usuarioLogado.Id, novoTipo))
                {
                    _usuarioParaEditar.TipoUsuario = novoTipo;
                }
            }
        }

        // Salvar alterações
        await _usuarioService.AtualizarUsuarioAsync(_usuarioParaEditar);
    }
}
