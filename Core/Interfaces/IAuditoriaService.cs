using HelpFastDesktop.Core.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface IAuditoriaService
{
    // Logs de auditoria
    Task LogAcaoAsync(string acao, string tabela, int? registroId, int? usuarioId, 
        object? dadosAntigos = null, object? dadosNovos = null, string? ipAddress = null, string? userAgent = null);
    
    Task<List<LogAuditoria>> ObterLogsAsync(DateTime dataInicio, DateTime dataFim, int? usuarioId = null, string? acao = null);
    Task<List<LogAuditoria>> ObterLogsPorTabelaAsync(string tabela, DateTime dataInicio, DateTime dataFim);
    Task<List<LogAuditoria>> ObterLogsPorUsuarioAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);
    
    // Conformidade LGPD
    Task<List<LogAuditoria>> ObterLogsAcessoDadosAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);
    Task<bool> ExcluirLogsAntigosAsync(DateTime dataLimite);
    Task<bool> AnonimizarLogsUsuarioAsync(int usuarioId);
    
    // Relatórios de auditoria
    Task<RelatorioAuditoria> GerarRelatorioAuditoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<string>> ObterAcoesRealizadasAsync(DateTime dataInicio, DateTime dataFim);
    Task<Dictionary<string, int>> ObterEstatisticasAcoesAsync(DateTime dataInicio, DateTime dataFim);
}

