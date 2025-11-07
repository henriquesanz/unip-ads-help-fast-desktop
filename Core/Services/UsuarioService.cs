using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
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
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var emailNormalizado = email.Trim().ToLower();
        
        // Busca case-insensitive do email - EF Core traduz ToLower() para LOWER() no SQL Server
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        // Validações básicas
        if (string.IsNullOrWhiteSpace(usuario.Nome))
            throw new ArgumentException("Nome é obrigatório", nameof(usuario));
        
        if (string.IsNullOrWhiteSpace(usuario.Email))
            throw new ArgumentException("Email é obrigatório", nameof(usuario));
        
        if (string.IsNullOrWhiteSpace(usuario.Senha))
            throw new ArgumentException("Senha é obrigatória", nameof(usuario));

        // Hash da senha será implementado em produção
        // usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        
        try
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"Usuário criado com sucesso: {usuario.Nome} (ID: {usuario.Id}, Email: {usuario.Email})");
            return usuario;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao criar usuário: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return false;

        try
        {
            var usuario = await ObterPorEmailAsync(email);
            
            if (usuario == null)
            {
                // Log para debug (pode ser removido em produção)
                System.Diagnostics.Debug.WriteLine($"Usuário não encontrado para email: {email}");
                return false;
            }

            // Verificação de hash da senha será implementada em produção
            // return BCrypt.Net.BCrypt.Verify(senha, usuario.Senha);
            
            // Por enquanto, comparação simples (apenas para desenvolvimento)
            // Comparação case-sensitive da senha
            var senhaValida = usuario.Senha == senha;
            
            if (!senhaValida)
            {
                System.Diagnostics.Debug.WriteLine($"Senha inválida para email: {email}");
            }
            
            return senhaValida;
        }
        catch (Exception ex)
        {
            // Log do erro para debug
            System.Diagnostics.Debug.WriteLine($"Erro ao validar login: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
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
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailNormalizado = email.Trim().ToLower();
        
        // Busca case-insensitive do email
        return await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == emailNormalizado);
    }

    // Métodos específicos por tipo de usuário
    public async Task<Usuario> CriarClienteAsync(Usuario cliente)
    {
        if (cliente == null)
            throw new ArgumentNullException(nameof(cliente));

        // Usar estratégia de execução para suportar retry com transações
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscar ou criar cargo Cliente
                var cargoCliente = await _context.Cargos.FirstOrDefaultAsync(c => c.Nome == "Cliente");
                if (cargoCliente == null)
                {
                    cargoCliente = new Cargo { Nome = "Cliente" };
                    _context.Cargos.Add(cargoCliente);
                    // Salvar cargo primeiro para obter o ID
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Cargo 'Cliente' criado no banco de dados");
                }
                cliente.CargoId = cargoCliente.Id;
                
                // Criar usuário
                var usuario = await CriarUsuarioAsync(cliente);
                
                await transaction.CommitAsync();
                System.Diagnostics.Debug.WriteLine($"Cliente criado com sucesso: {usuario.Nome} (ID: {usuario.Id})");
                return usuario;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Erro ao criar cliente: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<Usuario> CriarTecnicoAsync(Usuario tecnico, int criadoPorId)
    {
        if (tecnico == null)
            throw new ArgumentNullException(nameof(tecnico));

        // Usar estratégia de execução para suportar retry com transações
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscar ou criar cargo Técnico
                var cargoTecnico = await _context.Cargos.FirstOrDefaultAsync(c => c.Nome == "Técnico");
                if (cargoTecnico == null)
                {
                    cargoTecnico = new Cargo { Nome = "Técnico" };
                    _context.Cargos.Add(cargoTecnico);
                    // Salvar cargo primeiro para obter o ID
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Cargo 'Técnico' criado no banco de dados");
                }
                tecnico.CargoId = cargoTecnico.Id;
                
                // Criar usuário
                var usuario = await CriarUsuarioAsync(tecnico);
                
                await transaction.CommitAsync();
                System.Diagnostics.Debug.WriteLine($"Técnico criado com sucesso: {usuario.Nome} (ID: {usuario.Id}, Criado por: {criadoPorId})");
                return usuario;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Erro ao criar técnico: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<Usuario> CriarAdministradorAsync(Usuario administrador, int criadoPorId)
    {
        if (administrador == null)
            throw new ArgumentNullException(nameof(administrador));

        // Usar estratégia de execução para suportar retry com transações
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscar ou criar cargo Administrador
                var cargoAdmin = await _context.Cargos.FirstOrDefaultAsync(c => c.Nome == "Administrador");
                if (cargoAdmin == null)
                {
                    cargoAdmin = new Cargo { Nome = "Administrador" };
                    _context.Cargos.Add(cargoAdmin);
                    // Salvar cargo primeiro para obter o ID
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Cargo 'Administrador' criado no banco de dados");
                }
                administrador.CargoId = cargoAdmin.Id;
                
                // Criar usuário
                var usuario = await CriarUsuarioAsync(administrador);
                
                await transaction.CommitAsync();
                System.Diagnostics.Debug.WriteLine($"Administrador criado com sucesso: {usuario.Nome} (ID: {usuario.Id}, Criado por: {criadoPorId})");
                return usuario;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Erro ao criar administrador: {ex.Message}");
                throw;
            }
        });
    }

    // Métodos de listagem por tipo
    public async Task<List<Usuario>> ListarTecnicosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.Cargo != null && (u.Cargo.Nome == "Técnico" || u.Cargo.Nome == "Tecnico"))
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarAdministradoresAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.Cargo != null && u.Cargo.Nome == "Administrador")
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarUsuariosPorTipoAsync(UserRole tipoUsuario)
    {
        var cargoNome = tipoUsuario switch
        {
            UserRole.Cliente => "Cliente",
            UserRole.Tecnico => "Técnico",
            UserRole.Administrador => "Administrador",
            _ => ""
        };

        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.Cargo != null && (u.Cargo.Nome == cargoNome || u.Cargo.Nome == cargoNome.Replace("é", "e")))
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

