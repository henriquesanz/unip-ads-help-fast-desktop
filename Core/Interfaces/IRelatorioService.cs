using HelpFastDesktop.Core.Models.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface IRelatorioService
{
    // CRUD de Relatórios
    Task<Relatorio?> ObterPorIdAsync(int id);
    Task<List<Relatorio>> ListarTodosAsync();
    Task<List<Relatorio>> ListarPorUsuarioAsync(int usuarioId);
    Task<Relatorio> CriarRelatorioAsync(Relatorio relatorio);
    Task<Relatorio> AtualizarRelatorioAsync(Relatorio relatorio);
    Task<bool> ExcluirRelatorioAsync(int id);

    // Geração de Relatórios
    Task<RelatorioPerformance> GerarRelatorioPerformanceAsync(DateTime dataInicio, DateTime dataFim, int? tecnicoId = null);
    Task<RelatorioVolume> GerarRelatorioVolumeAsync(DateTime dataInicio, DateTime dataFim);
    Task<RelatorioSatisfacao> GerarRelatorioSatisfacaoAsync(DateTime dataInicio, DateTime dataFim);
    Task<RelatorioCustomizado> GerarRelatorioCustomizadoAsync(Relatorio relatorio);

    // Métricas e KPIs
    Task<Metrica> CalcularMetricasDiariasAsync(DateTime data);
    Task<List<Metrica>> CalcularMetricasPeriodoAsync(DateTime dataInicio, DateTime dataFim, string tipoMetrica);
    Task<DashboardMetrics> ObterMetricasDashboardAsync();

    // Agendamento
    Task<List<Relatorio>> ObterRelatoriosAgendadosAsync();
    Task ProcessarRelatoriosAgendadosAsync();
    Task AgendarRelatorioAsync(Relatorio relatorio);

    // Exportação
}

// DTOs para relatórios
public class RelatorioPerformance
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int? TecnicoId { get; set; }
    public string? TecnicoNome { get; set; }
    
    public int TotalChamados { get; set; }
    public int ChamadosResolvidos { get; set; }
    public int ChamadosEmAndamento { get; set; }
    public int ChamadosAtrasados { get; set; }
    
    public decimal TempoMedioResolucao { get; set; } // em horas
    public decimal TaxaResolucao { get; set; } // percentual
    public decimal SatisfacaoMedia { get; set; } // 1-5
    
    public List<PerformancePorPrioridade> PerformancePorPrioridade { get; set; } = new();
    public List<PerformancePorCategoria> PerformancePorCategoria { get; set; } = new();
    public List<ChamadoDetalhado> ChamadosDetalhados { get; set; } = new();
}

public class RelatorioVolume
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    
    public int TotalChamados { get; set; }
    public int ChamadosAbertos { get; set; }
    public int ChamadosResolvidos { get; set; }
    public int ChamadosFechados { get; set; }
    
    public List<VolumePorDia> VolumePorDia { get; set; } = new();
    public List<VolumePorPrioridade> VolumePorPrioridade { get; set; } = new();
    public List<VolumePorCategoria> VolumePorCategoria { get; set; } = new();
    public List<VolumePorUsuario> VolumePorUsuario { get; set; } = new();
    public List<VolumePorTecnico> VolumePorTecnico { get; set; } = new();
}

public class RelatorioSatisfacao
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    
    public int TotalAvaliacoes { get; set; }
    public decimal SatisfacaoMedia { get; set; }
    public int Avaliacoes5 { get; set; }
    public int Avaliacoes4 { get; set; }
    public int Avaliacoes3 { get; set; }
    public int Avaliacoes2 { get; set; }
    public int Avaliacoes1 { get; set; }
    
    public List<SatisfacaoPorTecnico> SatisfacaoPorTecnico { get; set; } = new();
    public List<SatisfacaoPorCategoria> SatisfacaoPorCategoria { get; set; } = new();
    public List<SatisfacaoPorPeriodo> SatisfacaoPorPeriodo { get; set; } = new();
    public List<ComentarioSatisfacao> Comentarios { get; set; } = new();
}

public class RelatorioCustomizado
{
    public int RelatorioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public Dictionary<string, object> Parametros { get; set; } = new();
    public List<object> Dados { get; set; } = new();
    public List<GraficoData> Graficos { get; set; } = new();
}

public class DashboardMetrics
{
    public int ChamadosAbertos { get; set; }
    public int ChamadosEmAndamento { get; set; }
    public int ChamadosResolvidosHoje { get; set; }
    public int ChamadosAtrasados { get; set; }
    
    public decimal TempoMedioResolucao { get; set; }
    public decimal SatisfacaoMedia { get; set; }
    public decimal TaxaResolucaoPrimeiroContato { get; set; }
    
    public List<MetricaPorHora> ChamadosPorHora { get; set; } = new();
    public List<MetricaPorDia> ChamadosPorDia { get; set; } = new();
    public List<MetricaPorCategoria> ChamadosPorCategoria { get; set; } = new();
    public List<MetricaPorPrioridade> ChamadosPorPrioridade { get; set; } = new();
}

// Classes auxiliares
public class PerformancePorPrioridade
{
    public string Prioridade { get; set; } = string.Empty;
    public int TotalChamados { get; set; }
    public int ChamadosResolvidos { get; set; }
    public decimal TempoMedioResolucao { get; set; }
    public decimal TaxaResolucao { get; set; }
}

public class PerformancePorCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public int TotalChamados { get; set; }
    public int ChamadosResolvidos { get; set; }
    public decimal TempoMedioResolucao { get; set; }
    public decimal SatisfacaoMedia { get; set; }
}

public class ChamadoDetalhado
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Prioridade { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public decimal TempoResolucao { get; set; }
    public int? Satisfacao { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string? TecnicoNome { get; set; }
}

public class VolumePorDia
{
    public DateTime Data { get; set; }
    public int ChamadosAbertos { get; set; }
    public int ChamadosResolvidos { get; set; }
    public int ChamadosFechados { get; set; }
}

public class VolumePorPrioridade
{
    public string Prioridade { get; set; } = string.Empty;
    public int Total { get; set; }
    public decimal Percentual { get; set; }
}

public class VolumePorCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public int Total { get; set; }
    public decimal Percentual { get; set; }
}

public class VolumePorUsuario
{
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int TotalChamados { get; set; }
}

public class VolumePorTecnico
{
    public int TecnicoId { get; set; }
    public string TecnicoNome { get; set; } = string.Empty;
    public int ChamadosAtribuidos { get; set; }
    public int ChamadosResolvidos { get; set; }
}

public class SatisfacaoPorTecnico
{
    public int TecnicoId { get; set; }
    public string TecnicoNome { get; set; } = string.Empty;
    public int TotalAvaliacoes { get; set; }
    public decimal SatisfacaoMedia { get; set; }
}

public class SatisfacaoPorCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public int TotalAvaliacoes { get; set; }
    public decimal SatisfacaoMedia { get; set; }
}

public class SatisfacaoPorPeriodo
{
    public DateTime Data { get; set; }
    public int TotalAvaliacoes { get; set; }
    public decimal SatisfacaoMedia { get; set; }
}

public class ComentarioSatisfacao
{
    public int ChamadoId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int Satisfacao { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime DataAvaliacao { get; set; }
}

public class MetricaPorHora
{
    public int Hora { get; set; }
    public int TotalChamados { get; set; }
}

public class MetricaPorDia
{
    public DateTime Data { get; set; }
    public int ChamadosAbertos { get; set; }
    public int ChamadosResolvidos { get; set; }
}

public class MetricaPorCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class MetricaPorPrioridade
{
    public string Prioridade { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class GraficoData
{
    public string Tipo { get; set; } = string.Empty; // linha, barra, pizza
    public string Titulo { get; set; } = string.Empty;
    public List<object> Dados { get; set; } = new();
    public Dictionary<string, object> Configuracoes { get; set; } = new();
}
