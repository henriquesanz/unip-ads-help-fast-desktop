using HelpFastDesktop.Services;

namespace HelpFastDesktop.Forms;

public partial class GerenciarUsuariosForm : Form
{
    private readonly ISessionService _sessionService;
    private readonly IUsuarioService _usuarioService;

    public GerenciarUsuariosForm(ISessionService sessionService, IUsuarioService usuarioService)
    {
        _sessionService = sessionService;
        _usuarioService = usuarioService;
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = "HELP FAST - Gerenciar Usuários";
        this.Size = new Size(1100, 700);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.BackColor = Color.FromArgb(45, 45, 48);

        SetupControls();
        LoadUsuarios();
    }

    private ListView? _usuariosListView;

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
            Text = "Gerenciar Usuários",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Height = 40,
            Dock = DockStyle.Top
        };

        // Panel de botões
        var buttonsPanel = new Panel
        {
            Height = 50,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(0, 10, 0, 10)
        };

        SetupButtons(buttonsPanel);

        // ListView para exibir usuários
        _usuariosListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.FromArgb(60, 60, 63),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9)
        };

        _usuariosListView.Columns.Add("ID", 50);
        _usuariosListView.Columns.Add("Nome", 200);
        _usuariosListView.Columns.Add("Email", 200);
        _usuariosListView.Columns.Add("Tipo", 100);
        _usuariosListView.Columns.Add("Telefone", 120);
        _usuariosListView.Columns.Add("Data Criação", 120);

        // Eventos do ListView
        _usuariosListView.DoubleClick += UsuariosListView_DoubleClick;
        _usuariosListView.KeyDown += UsuariosListView_KeyDown;

        mainPanel.Controls.Add(_usuariosListView);
        mainPanel.Controls.Add(buttonsPanel);
        mainPanel.Controls.Add(titleLabel);

        this.Controls.Add(mainPanel);
    }

    private void SetupButtons(Panel buttonsPanel)
    {
        var novoUsuarioButton = new Button
        {
            Text = "NOVO USUÁRIO",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 120, 215),
            Size = new Size(130, 30),
            Location = new Point(10, 10),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        novoUsuarioButton.FlatAppearance.BorderSize = 0;
        novoUsuarioButton.Click += NovoUsuarioButton_Click;

        var editarButton = new Button
        {
            Text = "EDITAR",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 150, 100),
            Size = new Size(100, 30),
            Location = new Point(150, 10),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        editarButton.FlatAppearance.BorderSize = 0;
        editarButton.Click += EditarButton_Click;

        var excluirButton = new Button
        {
            Text = "EXCLUIR",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(200, 50, 50),
            Size = new Size(100, 30),
            Location = new Point(260, 10),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        excluirButton.FlatAppearance.BorderSize = 0;
        excluirButton.Click += ExcluirButton_Click;

        var refreshButton = new Button
        {
            Text = "ATUALIZAR",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(100, 100, 100),
            Size = new Size(100, 30),
            Location = new Point(370, 10),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (s, e) => LoadUsuarios();

        // Armazenar referências para habilitar/desabilitar
        _editarButton = editarButton;
        _excluirButton = excluirButton;

        buttonsPanel.Controls.Add(novoUsuarioButton);
        buttonsPanel.Controls.Add(editarButton);
        buttonsPanel.Controls.Add(excluirButton);
        buttonsPanel.Controls.Add(refreshButton);
    }

    private Button? _editarButton;
    private Button? _excluirButton;

    private async void LoadUsuarios()
    {
        try
        {
            if (_usuariosListView == null) return;

            _usuariosListView.Items.Clear();
            _usuariosListView.SelectedItems.Clear();

            // Habilitar/desabilitar botões baseado na seleção
            UpdateButtonStates();

            var usuarios = await _usuarioService.ListarUsuariosPorTipoAsync(Models.UserRole.Cliente);
            usuarios.AddRange(await _usuarioService.ListarTecnicosAsync());
            usuarios.AddRange(await _usuarioService.ListarAdministradoresAsync());
            
            foreach (var usuario in usuarios)
            {
                var item = new ListViewItem(usuario.Id.ToString());
                item.SubItems.Add(usuario.Nome);
                item.SubItems.Add(usuario.Email);
                item.SubItems.Add(usuario.TipoUsuarioDisplay);
                item.SubItems.Add(usuario.Telefone);
                item.SubItems.Add(usuario.DataCriacao.ToString("dd/MM/yyyy"));
                item.Tag = usuario; // Armazenar o objeto usuário no Tag
                _usuariosListView.Items.Add(item);
            }

            // Adicionar evento de seleção
            _usuariosListView.SelectedIndexChanged += (s, e) => UpdateButtonStates();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateButtonStates()
    {
        if (_editarButton == null || _excluirButton == null || _usuariosListView == null) return;

        var hasSelection = _usuariosListView.SelectedItems.Count > 0;
        _editarButton.Enabled = hasSelection;
        _excluirButton.Enabled = hasSelection;
    }

    private void NovoUsuarioButton_Click(object? sender, EventArgs e)
    {
        try
        {
            var usuarioLogado = _sessionService.UsuarioLogado;
            if (usuarioLogado == null) return;

            var cadastroForm = new CadastroUsuarioForm(_sessionService, _usuarioService, Models.UserRole.Cliente);
            if (cadastroForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsuarios(); // Recarregar lista
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir cadastro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditarButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_usuariosListView?.SelectedItems.Count == 0) return;

            var selectedItem = _usuariosListView.SelectedItems[0];
            var usuario = selectedItem.Tag as Models.Usuario;
            if (usuario == null) return;

            var edicaoForm = new CadastroUsuarioForm(_sessionService, _usuarioService, usuario.TipoUsuario, usuario);
            if (edicaoForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsuarios(); // Recarregar lista
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao editar usuário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ExcluirButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_usuariosListView?.SelectedItems.Count == 0) return;

            var selectedItem = _usuariosListView.SelectedItems[0];
            var usuario = selectedItem.Tag as Models.Usuario;
            if (usuario == null) return;

            // Não permitir excluir o próprio usuário
            var usuarioLogado = _sessionService.UsuarioLogado;
            if (usuarioLogado != null && usuario.Id == usuarioLogado.Id)
            {
                MessageBox.Show("Você não pode excluir seu próprio usuário.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Deseja realmente excluir o usuário '{usuario.Nome}'?\n\nEsta ação não pode ser desfeita.",
                "Confirmar Exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: Implementar método de exclusão no UsuarioService
                // await _usuarioService.ExcluirUsuarioAsync(usuario.Id);
                usuario.Ativo = false;
                await _usuarioService.AtualizarUsuarioAsync(usuario);
                
                MessageBox.Show("Usuário excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsuarios(); // Recarregar lista
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao excluir usuário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UsuariosListView_DoubleClick(object? sender, EventArgs e)
    {
        EditarButton_Click(sender, e);
    }

    private void UsuariosListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            ExcluirButton_Click(sender, e);
        }
        else if (e.KeyCode == Keys.Enter)
        {
            EditarButton_Click(sender, e);
        }
    }
}

