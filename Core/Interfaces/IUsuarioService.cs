using HelpFastDesktop.Core.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario> CriarUsuarioAsync(Usuario usuario);
    Task<Usuario> AtualizarUsuarioAsync(Usuario usuario);
    Task<bool> ValidarLoginAsync(string email, string senha);
    Task AtualizarUltimoLoginAsync(int usuarioId);
    Task<bool> EmailExisteAsync(string email);
    
    // Métodos específicos por tipo de usuário
    Task<Usuario> CriarClienteAsync(Usuario cliente);
    Task<Usuario> CriarTecnicoAsync(Usuario tecnico, int criadoPorId);
    Task<Usuario> CriarAdministradorAsync(Usuario administrador, int criadoPorId);
    
    // Métodos de listagem por tipo
    Task<List<Usuario>> ListarTecnicosAsync();
    Task<List<Usuario>> ListarAdministradoresAsync();
    Task<List<Usuario>> ListarUsuariosPorTipoAsync(UserRole tipoUsuario);
    
    // Validações de permissão
    Task<bool> PodeCriarUsuarioAsync(int usuarioId, UserRole tipoUsuarioParaCriar);
    Task<bool> PodeGerenciarUsuariosAsync(int usuarioId);
}

