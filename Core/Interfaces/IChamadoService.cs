using HelpFastDesktop.Core.Models.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface IChamadoService
{
    // Métodos básicos CRUD
    Task<Chamado?> ObterPorIdAsync(int id);
    Task<Chamado> CriarChamadoAsync(Chamado chamado);
    Task<Chamado> AtualizarChamadoAsync(Chamado chamado);
    Task<bool> ExcluirChamadoAsync(int id);
    
    // Métodos de listagem
    Task<List<Chamado>> ListarChamadosDoUsuarioAsync(int usuarioId);
    Task<List<Chamado>> ListarTodosChamadosAsync();
    Task<List<Chamado>> ListarChamadosPorStatusAsync(string status);
    Task<List<Chamado>> ListarChamadosPorPrioridadeAsync(string prioridade);
    
    // Métodos específicos para técnicos
    Task<Chamado> AtribuirChamadoAsync(int chamadoId, int tecnicoId);
    Task<Chamado> ResolverChamadoAsync(int chamadoId, string observacoes);
    Task<List<Chamado>> ListarChamadosAtribuidosAsync(int tecnicoId);
    
    // Validações
    Task<bool> PodeVisualizarChamadoAsync(int usuarioId, int chamadoId);
    Task<bool> PodeEditarChamadoAsync(int usuarioId, int chamadoId);
}
