using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

public class ChamadoService : IChamadoService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsuarioService _usuarioService;

    public ChamadoService(ApplicationDbContext context, IUsuarioService usuarioService)
    {
        _context = context;
        _usuarioService = usuarioService;
    }

    public async Task<Chamado?> ObterPorIdAsync(int id)
    {
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Chamado> CriarChamadoAsync(Chamado chamado)
    {
        _context.Chamados.Add(chamado);
        await _context.SaveChangesAsync();
        return chamado;
    }

    public async Task<Chamado> AtualizarChamadoAsync(Chamado chamado)
    {
        _context.Chamados.Update(chamado);
        await _context.SaveChangesAsync();
        return chamado;
    }

    public async Task<bool> ExcluirChamadoAsync(int id)
    {
        var chamado = await _context.Chamados.FindAsync(id);
        if (chamado == null) return false;

        _context.Chamados.Remove(chamado);
        await _context.SaveChangesAsync();
        return true;
    }

    // Métodos de listagem
    public async Task<List<Chamado>> ListarChamadosDoUsuarioAsync(int usuarioId)
    {
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .Where(c => c.ClienteId == usuarioId)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();
    }

    public async Task<List<Chamado>> ListarTodosChamadosAsync()
    {
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();
    }

    public async Task<List<Chamado>> ListarChamadosPorStatusAsync(string status)
    {
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();
    }

    public async Task<List<Chamado>> ListarChamadosPorPrioridadeAsync(string prioridade)
    {
        // Prioridade não existe mais na tabela, retornando todos
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();
    }

    // Métodos específicos para técnicos
    public async Task<Chamado> AtribuirChamadoAsync(int chamadoId, int tecnicoId)
    {
        var chamado = await _context.Chamados.FindAsync(chamadoId);
        if (chamado == null) throw new ArgumentException("Chamado não encontrado");

        chamado.TecnicoId = tecnicoId;
        chamado.Status = "EmAndamento";

        await _context.SaveChangesAsync();
        return chamado;
    }

    public async Task<Chamado> ResolverChamadoAsync(int chamadoId, string observacoes)
    {
        var chamado = await _context.Chamados.FindAsync(chamadoId);
        if (chamado == null) throw new ArgumentException("Chamado não encontrado");

        chamado.Status = "Resolvido";
        chamado.DataFechamento = DateTime.Now;

        await _context.SaveChangesAsync();
        return chamado;
    }

    public async Task<List<Chamado>> ListarChamadosAtribuidosAsync(int tecnicoId)
    {
        return await _context.Chamados
            .Include(c => c.Cliente)
            .Include(c => c.Tecnico)
            .Where(c => c.TecnicoId == tecnicoId)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();
    }

    // Validações
    public async Task<bool> PodeVisualizarChamadoAsync(int usuarioId, int chamadoId)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(usuarioId);
        if (usuario == null) return false;

        var chamado = await _context.Chamados.FindAsync(chamadoId);
        if (chamado == null) return false;

        // Cliente só pode ver seus próprios chamados
        if (usuario.TipoUsuario == UserRole.Cliente)
            return chamado.ClienteId == usuarioId;

        // Técnico e Administrador podem ver todos
        return usuario.PodeVerTodosChamados;
    }

    public async Task<bool> PodeEditarChamadoAsync(int usuarioId, int chamadoId)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(usuarioId);
        if (usuario == null) return false;

        var chamado = await _context.Chamados.FindAsync(chamadoId);
        if (chamado == null) return false;

        // Apenas técnicos e administradores podem editar chamados
        if (usuario.TipoUsuario == UserRole.Cliente)
            return false;

        // Só pode editar se o chamado não estiver resolvido
        return chamado.PodeSerEditado;
    }
}

