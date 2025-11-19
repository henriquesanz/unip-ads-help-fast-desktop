using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Security;

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
        {
            Console.WriteLine("[USUARIO][ERRO] Email vazio ao buscar usuário.");
            return null;
        }

        var emailNormalizado = email.Trim().ToLower();
        Console.WriteLine($"[USUARIO] Buscando usuário por email normalizado: '{emailNormalizado}'.");
        
        // Busca case-insensitive do email - EF Core traduz ToLower() para LOWER() no SQL Server
        var usuario = await _context.Usuarios
            .Include(u => u.Cargo)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

        if (usuario == null)
        {
            Console.WriteLine("[USUARIO] Nenhum usuário encontrado para o email informado.");
        }
        else
        {
            Console.WriteLine($"[USUARIO] Usuário localizado. ID: {usuario.Id}, Nome: {usuario.Nome}, Cargo: {usuario.Cargo?.Nome ?? "Sem cargo"}.");
        }

        return usuario;
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

        if (!PasswordHasher.IsHashed(usuario.Senha))
        {
            usuario.Senha = PasswordHasher.Hash(usuario.Senha);
        }
        
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
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        if (!PasswordHasher.IsHashed(usuario.Senha))
        {
            usuario.Senha = PasswordHasher.Hash(usuario.Senha);
        }
        
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

            var senhaValida = PasswordHasher.Verify(senha, usuario.Senha);

            if (!senhaValida && usuario.Senha == senha)
            {
                senhaValida = true;

                try
                {
                    usuario.Senha = PasswordHasher.Hash(senha);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Senha em texto plano migrada para hash para o usuário ID {usuario.Id}.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao atualizar hash da senha para o usuário ID {usuario.Id}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
            }

            if (senhaValida && !PasswordHasher.IsCurrentFormat(usuario.Senha))
            {
                try
                {
                    usuario.Senha = PasswordHasher.Hash(senha);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Senha do usuário ID {usuario.Id} migrada para o formato de hash atual.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao migrar hash da senha para o usuário ID {usuario.Id}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
            }

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
        await _context.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(u => u.UltimoLogin, _ => DateTime.Now));
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
                // IMPORTANTE: Técnicos devem sempre usar CargoId = 2
                // Primeiro, verificar se o cargo com ID 2 existe
                var cargoId2 = await _context.Cargos.FirstOrDefaultAsync(c => c.Id == 2);
                
                if (cargoId2 != null)
                {
                    // Cargo com ID 2 existe - usar ele (não importa o nome)
                    tecnico.CargoId = 2;
                    System.Diagnostics.Debug.WriteLine($"Cargo com ID 2 encontrado. Técnico será criado com CargoId = 2.");
                }
                else
                {
                    // Cargo com ID 2 não existe - buscar cargo "Técnico" pelo nome
                    var cargoTecnico = await _context.Cargos.FirstOrDefaultAsync(c => c.Nome == "Técnico");
                    
                    if (cargoTecnico != null)
                    {
                        // Cargo "Técnico" existe - usar o ID dele
                        tecnico.CargoId = cargoTecnico.Id;
                        System.Diagnostics.Debug.WriteLine($"Cargo 'Técnico' encontrado com ID {cargoTecnico.Id}. Técnico será criado com CargoId = {cargoTecnico.Id}.");
                        if (cargoTecnico.Id != 2)
                        {
                            System.Diagnostics.Debug.WriteLine($"ATENÇÃO: Cargo 'Técnico' deveria ter ID 2, mas tem ID {cargoTecnico.Id}. Considere corrigir o banco de dados.");
                        }
                    }
                    else
                    {
                        // Nem cargo com ID 2 nem cargo "Técnico" existem - criar novo
                        var novoCargo = new Cargo { Nome = "Técnico" };
                        _context.Cargos.Add(novoCargo);
                        await _context.SaveChangesAsync();
                        
                        tecnico.CargoId = novoCargo.Id;
                        System.Diagnostics.Debug.WriteLine($"Cargo 'Técnico' criado com ID {novoCargo.Id}. Técnico será criado com CargoId = {novoCargo.Id}.");
                    }
                }
                
                // Criar usuário
                var usuario = await CriarUsuarioAsync(tecnico);
                
                await transaction.CommitAsync();
                System.Diagnostics.Debug.WriteLine($"Técnico criado com sucesso: {usuario.Nome} (ID: {usuario.Id}, CargoId: {usuario.CargoId}, Criado por: {criadoPorId})");
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
        // CargoId 2 = Técnico
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.CargoId == 2)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarAdministradoresAsync()
    {
        // CargoId 1 = Administrador/Admin
        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.CargoId == 1)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarUsuariosPorTipoAsync(UserRole tipoUsuario)
    {
        // Mapeamento: 1 = Admin, 2 = Técnico, 3 = Cliente
        var cargoId = tipoUsuario switch
        {
            UserRole.Cliente => 3,
            UserRole.Tecnico => 2,
            UserRole.Administrador => 1,
            _ => 0
        };

        return await _context.Usuarios
            .Include(u => u.Cargo)
            .Where(u => u.CargoId == cargoId)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    // Validações de permissão
    public async Task RemoverUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        _context.Usuarios.Remove(usuario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao remover usuário ID {usuarioId}: {ex.Message}");
            throw new InvalidOperationException("Não foi possível excluir o usuário. Verifique se existem registros associados a ele.", ex);
        }
    }

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

