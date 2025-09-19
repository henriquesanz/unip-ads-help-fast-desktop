using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;

    public UsuarioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.Ativo);
    }

    public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
    {
        // Hash da senha será implementado em produção
        // usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> AtualizarUsuarioAsync(Usuario usuario)
    {
        // Hash da senha será implementado em produção se senha foi alterada
        // if (senhaFoiAlterada)
        //     usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> ValidarLoginAsync(string email, string senha)
    {
        var usuario = await ObterPorEmailAsync(email);
        
        if (usuario == null)
            return false;

        // Verificação de hash da senha será implementada em produção
        // return BCrypt.Net.BCrypt.Verify(senha, usuario.Senha);
        
        // Por enquanto, comparação simples (apenas para desenvolvimento)
        return usuario.Senha == senha;
    }

    public async Task AtualizarUltimoLoginAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.UltimoLogin = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email);
    }

    // Métodos específicos por tipo de usuário
    public async Task<Usuario> CriarClienteAsync(Usuario cliente)
    {
        cliente.TipoUsuario = UserRole.Cliente;
        cliente.CriadoPorId = null; // Clientes se auto-cadastram
        return await CriarUsuarioAsync(cliente);
    }

    public async Task<Usuario> CriarTecnicoAsync(Usuario tecnico, int criadoPorId)
    {
        tecnico.TipoUsuario = UserRole.Tecnico;
        tecnico.CriadoPorId = criadoPorId;
        return await CriarUsuarioAsync(tecnico);
    }

    public async Task<Usuario> CriarAdministradorAsync(Usuario administrador, int criadoPorId)
    {
        administrador.TipoUsuario = UserRole.Administrador;
        administrador.CriadoPorId = criadoPorId;
        return await CriarUsuarioAsync(administrador);
    }

    // Métodos de listagem por tipo
    public async Task<List<Usuario>> ListarTecnicosAsync()
    {
        return await _context.Usuarios
            .Where(u => u.TipoUsuario == UserRole.Tecnico && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarAdministradoresAsync()
    {
        return await _context.Usuarios
            .Where(u => u.TipoUsuario == UserRole.Administrador && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarUsuariosPorTipoAsync(UserRole tipoUsuario)
    {
        return await _context.Usuarios
            .Where(u => u.TipoUsuario == tipoUsuario && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    // Validações de permissão
    public async Task<bool> PodeCriarUsuarioAsync(int usuarioId, UserRole tipoUsuarioParaCriar)
    {
        var usuario = await ObterPorIdAsync(usuarioId);
        if (usuario == null) return false;

        return tipoUsuarioParaCriar switch
        {
            UserRole.Cliente => true, // Qualquer um pode criar cliente (auto-cadastro)
            UserRole.Tecnico => usuario.TipoUsuario == UserRole.Administrador,
            UserRole.Administrador => usuario.TipoUsuario == UserRole.Administrador,
            _ => false
        };
    }

    public async Task<bool> PodeGerenciarUsuariosAsync(int usuarioId)
    {
        var usuario = await ObterPorIdAsync(usuarioId);
        return usuario?.TipoUsuario == UserRole.Administrador;
    }
}

