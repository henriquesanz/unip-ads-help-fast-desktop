using HelpFastDesktop.Commands;
using HelpFastDesktop.Services;
using HelpFastDesktop.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace HelpFastDesktop.ViewModels;

public class CadastroUsuarioViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private readonly IUsuarioService _usuarioService;
    private readonly UserRole _tipoUsuarioPermitido;
    private readonly Usuario? _usuarioParaEditar;

    private string _nome = string.Empty;
    private string _email = string.Empty;
    private string _telefone = string.Empty;
    private string _senha = string.Empty;
    private UserRole _tipoUsuarioSelecionado;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public CadastroUsuarioViewModel(
        ISessionService sessionService, 
        IUsuarioService usuarioService, 
        UserRole tipoUsuarioPermitido = UserRole.Cliente, 
        Usuario? usuarioParaEditar = null)
    {
        _sessionService = sessionService;
        _usuarioService = usuarioService;
        _tipoUsuarioPermitido = tipoUsuarioPermitido;
        _usuarioParaEditar = usuarioParaEditar;

        SalvarCommand = new RelayCommand(async () => await SalvarAsync(), () => !IsLoading);
        CancelarCommand = new RelayCommand(() => Cancelar());

        // Configurar tipos de usuário disponíveis
        TiposUsuarioDisponiveis = new ObservableCollection<UserRole>();
        ConfigureTiposUsuarioDisponiveis();

        // Preencher campos se estiver editando
        if (_usuarioParaEditar != null)
        {
            Nome = _usuarioParaEditar.Nome;
            Email = _usuarioParaEditar.Email;
            Telefone = _usuarioParaEditar.Telefone;
            Senha = string.Empty; // Senha em branco para manter a atual
            TipoUsuarioSelecionado = _usuarioParaEditar.TipoUsuario;
        }
        else
        {
            TipoUsuarioSelecionado = _tipoUsuarioPermitido;
        }
    }

    public string Nome
    {
        get => _nome;
        set => SetProperty(ref _nome, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Telefone
    {
        get => _telefone;
        set => SetProperty(ref _telefone, value);
    }

    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    public UserRole TipoUsuarioSelecionado
    {
        get => _tipoUsuarioSelecionado;
        set => SetProperty(ref _tipoUsuarioSelecionado, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsEditando => _usuarioParaEditar != null;
    public string TituloFormulario => IsEditando ? "Editar Usuário" : "Cadastro de Usuário";
    public string TextoBotaoSalvar => IsEditando ? "ATUALIZAR" : "CADASTRAR";
    public bool MostrarTipoUsuario
    {
        get
        {
            var usuarioLogado = _sessionService.UsuarioLogado;
            // Só mostra o campo se há usuário logado (não é cadastro inicial)
            return usuarioLogado != null;
        }
    }

    public ObservableCollection<UserRole> TiposUsuarioDisponiveis { get; }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event Action? SalvarSuccessful;
    public event Action? CancelarRequested;

    private void ConfigureTiposUsuarioDisponiveis()
    {
        var usuarioLogado = _sessionService.UsuarioLogado;
        
        // Se não há usuário logado (cadastro inicial), apenas Cliente
        if (usuarioLogado == null)
        {
            TiposUsuarioDisponiveis.Add(UserRole.Cliente);
        }
        // Se usuário logado é Administrador, pode criar todos os tipos
        else if (usuarioLogado.TipoUsuario == UserRole.Administrador)
        {
            TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            TiposUsuarioDisponiveis.Add(UserRole.Tecnico);
            TiposUsuarioDisponiveis.Add(UserRole.Administrador);
        }
        // Se usuário logado é Técnico, pode criar Cliente e Técnico
        else if (usuarioLogado.TipoUsuario == UserRole.Tecnico)
        {
            TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            TiposUsuarioDisponiveis.Add(UserRole.Tecnico);
        }
        // Outros casos, apenas Cliente
        else
        {
            TiposUsuarioDisponiveis.Add(UserRole.Cliente);
        }
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            ErrorMessage = "Nome é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "E-mail é obrigatório.";
            return false;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "E-mail inválido.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Telefone))
        {
            ErrorMessage = "Telefone é obrigatório.";
            return false;
        }

        if (!IsEditando && string.IsNullOrWhiteSpace(Senha))
        {
            ErrorMessage = "Senha é obrigatória para novos usuários.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Senha) && Senha.Length < 6)
        {
            ErrorMessage = "Senha deve ter pelo menos 6 caracteres.";
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

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (_usuarioParaEditar != null)
            {
                await AtualizarUsuarioAsync();
            }
            else
            {
                await CriarUsuarioAsync();
            }

            SalvarSuccessful?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao salvar usuário: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CriarUsuarioAsync()
    {
        var usuario = new Usuario
        {
            Nome = Nome.Trim(),
            Email = Email.Trim().ToLower(),
            Telefone = Telefone.Trim(),
            Senha = Senha,
            DataCriacao = DateTime.Now,
            Ativo = true
        };

        // Verificar se e-mail já existe
        if (await _usuarioService.EmailExisteAsync(usuario.Email))
        {
            throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
        }

        var usuarioLogado = _sessionService.UsuarioLogado;
        
        // Definir tipo de usuário baseado no contexto
        UserRole tipoUsuarioFinal;
        if (usuarioLogado == null)
        {
            // Cadastro inicial - sempre Cliente por segurança
            tipoUsuarioFinal = UserRole.Cliente;
        }
        else
        {
            // Usuário logado - usar o tipo selecionado
            tipoUsuarioFinal = TipoUsuarioSelecionado;
            
            // Verificar permissões
            if (!await _usuarioService.PodeCriarUsuarioAsync(usuarioLogado.Id, tipoUsuarioFinal))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para criar usuários deste tipo.");
            }
        }

        // Criar usuário baseado no tipo
        switch (tipoUsuarioFinal)
        {
            case UserRole.Cliente:
                await _usuarioService.CriarClienteAsync(usuario);
                break;
            case UserRole.Tecnico:
                await _usuarioService.CriarTecnicoAsync(usuario, usuarioLogado!.Id);
                break;
            case UserRole.Administrador:
                await _usuarioService.CriarAdministradorAsync(usuario, usuarioLogado!.Id);
                break;
        }
    }

    private async Task AtualizarUsuarioAsync()
    {
        if (_usuarioParaEditar == null) return;

        // Verificar se e-mail mudou e se já existe
        if (_usuarioParaEditar.Email != Email.Trim().ToLower())
        {
            if (await _usuarioService.EmailExisteAsync(Email.Trim().ToLower()))
            {
                throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
            }
        }

        // Atualizar dados
        _usuarioParaEditar.Nome = Nome.Trim();
        _usuarioParaEditar.Email = Email.Trim().ToLower();
        _usuarioParaEditar.Telefone = Telefone.Trim();

        // Atualizar senha se fornecida
        if (!string.IsNullOrWhiteSpace(Senha))
        {
            _usuarioParaEditar.Senha = Senha;
        }

        // Atualizar tipo se permitido
        var usuarioLogado = _sessionService.UsuarioLogado;
        if (usuarioLogado?.TipoUsuario == UserRole.Administrador && TipoUsuarioSelecionado != _usuarioParaEditar.TipoUsuario)
        {
            // Verificar permissões para alterar tipo
            if (usuarioLogado != null && await _usuarioService.PodeCriarUsuarioAsync(usuarioLogado.Id, TipoUsuarioSelecionado))
            {
                _usuarioParaEditar.TipoUsuario = TipoUsuarioSelecionado;
            }
        }

        // Salvar alterações
        await _usuarioService.AtualizarUsuarioAsync(_usuarioParaEditar);
    }

    private void Cancelar()
    {
        CancelarRequested?.Invoke();
    }
}
