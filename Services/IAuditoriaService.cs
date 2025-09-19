using HelpFastDesktop.Models;

namespace HelpFastDesktop.Services;

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

// DTOs para auditoria
public class RelatorioAuditoria
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int TotalAcoes { get; set; }
    public int UsuariosAtivos { get; set; }
    public Dictionary<string, int> AcoesPorTipo { get; set; } = new();
    public Dictionary<string, int> AcoesPorTabela { get; set; } = new();
    public Dictionary<string, int> AcoesPorUsuario { get; set; } = new();
    public List<LogAuditoria> LogsCriticos { get; set; } = new();
    public List<AtividadeUsuario> AtividadeUsuarios { get; set; } = new();
}

public class AtividadeUsuario
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int TotalAcoes { get; set; }
    public DateTime PrimeiraAcao { get; set; }
    public DateTime UltimaAcao { get; set; }
    public List<string> TiposAcoes { get; set; } = new();
    public List<string> TabelasAcessadas { get; set; } = new();
}
