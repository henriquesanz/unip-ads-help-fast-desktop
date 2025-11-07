using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HelpFastDesktop.Presentation.Controllers;

public class CadastroUsuarioController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly IUsuarioService _usuarioService;
    private readonly UserRole _tipoUsuarioPermitido;
    private readonly Usuario? _usuarioParaEditar;
    private CadastroUsuarioModel _model;

    public CadastroUsuarioController(
        IServiceProvider serviceProvider,
        UserRole tipoUsuarioPermitido = UserRole.Cliente,
        Usuario? usuarioParaEditar = null) : base(serviceProvider)
    {
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _usuarioService = serviceProvider.GetRequiredService<IUsuarioService>();
        _tipoUsuarioPermitido = tipoUsuarioPermitido;
        _usuarioParaEditar = usuarioParaEditar;
        _model = new CadastroUsuarioModel();

        ConfigureTiposUsuarioDisponiveis();

        if (_usuarioParaEditar != null)
        {
            _model.Nome = _usuarioParaEditar.Nome;
            _model.Email = _usuarioParaEditar.Email;
            _model.Telefone = _usuarioParaEditar.Telefone;
            _model.Senha = string.Empty;
            _model.TipoUsuarioSelecionado = _usuarioParaEditar.TipoUsuario;
            _model.IsEditando = true;
            _model.TituloFormulario = "Editar Usuário";
            _model.TextoBotaoSalvar = "ATUALIZAR";
        }
        else
        {
            _model.TipoUsuarioSelecionado = _tipoUsuarioPermitido;
            _model.IsEditando = false;
            _model.TituloFormulario = "Cadastro de Usuário";
            _model.TextoBotaoSalvar = "CADASTRAR";
        }
    }

    public CadastroUsuarioModel GetModel() => _model;

    public void SetNome(string nome)
    {
        _model.Nome = nome;
    }

    public void SetEmail(string email)
    {
        _model.Email = email;
    }

    public void SetTelefone(string telefone)
    {
        _model.Telefone = telefone;
    }

    public void SetSenha(string senha)
    {
        _model.Senha = senha;
    }

    public void SetTipoUsuarioSelecionado(UserRole tipoUsuario)
    {
        _model.TipoUsuarioSelecionado = tipoUsuario;
    }

    private void ConfigureTiposUsuarioDisponiveis()
    {
        var usuarioLogado = _sessionService.UsuarioLogado;
        
        if (usuarioLogado == null)
        {
            _model.TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            _model.MostrarTipoUsuario = false;
        }
        else if (usuarioLogado.TipoUsuario == UserRole.Administrador)
        {
            _model.TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            _model.TiposUsuarioDisponiveis.Add(UserRole.Tecnico);
            _model.TiposUsuarioDisponiveis.Add(UserRole.Administrador);
            _model.MostrarTipoUsuario = true;
        }
        else if (usuarioLogado.TipoUsuario == UserRole.Tecnico)
        {
            _model.TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            _model.TiposUsuarioDisponiveis.Add(UserRole.Tecnico);
            _model.MostrarTipoUsuario = true;
        }
        else
        {
            _model.TiposUsuarioDisponiveis.Add(UserRole.Cliente);
            _model.MostrarTipoUsuario = false;
        }
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(_model.Nome))
        {
            _model.ErrorMessage = "Nome é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_model.Email))
        {
            _model.ErrorMessage = "E-mail é obrigatório.";
            return false;
        }

        if (!IsValidEmail(_model.Email))
        {
            _model.ErrorMessage = "E-mail inválido.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_model.Telefone))
        {
            _model.ErrorMessage = "Telefone é obrigatório.";
            return false;
        }

        if (!_model.IsEditando && string.IsNullOrWhiteSpace(_model.Senha))
        {
            _model.ErrorMessage = "Senha é obrigatória para novos usuários.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_model.Senha) && _model.Senha.Length < 6)
        {
            _model.ErrorMessage = "Senha deve ter pelo menos 6 caracteres.";
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

    public async System.Threading.Tasks.Task SalvarAsync()
    {
        if (!ValidarCampos())
            return;

        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            if (_usuarioParaEditar != null)
            {
                await AtualizarUsuarioAsync();
            }
            else
            {
                await CriarUsuarioAsync();
            }

            OnSalvarSuccessful?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            // Exceções de validação de negócio (ex: email já existe)
            _model.ErrorMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine($"Erro de validação ao salvar usuário: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Exceções de permissão
            _model.ErrorMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine($"Erro de permissão ao salvar usuário: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            // Exceções de argumentos inválidos
            _model.ErrorMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine($"Erro de argumento ao salvar usuário: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Outras exceções (erros de banco, etc)
            _model.ErrorMessage = $"Erro ao salvar usuário: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Erro ao salvar usuário: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    private async System.Threading.Tasks.Task CriarUsuarioAsync()
    {
        var usuario = new Usuario
        {
            Nome = _model.Nome.Trim(),
            Email = _model.Email.Trim().ToLower(),
            Telefone = _model.Telefone.Trim(),
            Senha = _model.Senha
        };

        if (await _usuarioService.EmailExisteAsync(usuario.Email))
        {
            throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
        }

        var usuarioLogado = _sessionService.UsuarioLogado;
        
        UserRole tipoUsuarioFinal;
        if (usuarioLogado == null)
        {
            tipoUsuarioFinal = UserRole.Cliente;
        }
        else
        {
            tipoUsuarioFinal = _model.TipoUsuarioSelecionado;
            
            if (!await _usuarioService.PodeCriarUsuarioAsync(usuarioLogado.Id, tipoUsuarioFinal))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para criar usuários deste tipo.");
            }
        }

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

    private async System.Threading.Tasks.Task AtualizarUsuarioAsync()
    {
        if (_usuarioParaEditar == null) return;

        if (_usuarioParaEditar.Email != _model.Email.Trim().ToLower())
        {
            if (await _usuarioService.EmailExisteAsync(_model.Email.Trim().ToLower()))
            {
                throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");
            }
        }

        _usuarioParaEditar.Nome = _model.Nome.Trim();
        _usuarioParaEditar.Email = _model.Email.Trim().ToLower();
        _usuarioParaEditar.Telefone = _model.Telefone.Trim();

        if (!string.IsNullOrWhiteSpace(_model.Senha))
        {
            _usuarioParaEditar.Senha = _model.Senha;
        }

        // Atualizar cargo se necessário (requer ajuste no serviço)
        // Por enquanto, apenas atualizamos os dados básicos

        await _usuarioService.AtualizarUsuarioAsync(_usuarioParaEditar);
    }

    public void Cancelar()
    {
        OnCancelarRequested?.Invoke();
    }

    public event Action? OnSalvarSuccessful;
    public event Action? OnCancelarRequested;
}
