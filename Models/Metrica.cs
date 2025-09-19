using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Models;

public class Metrica
{
    public int Id { get; set; }

    [Required]
    public DateTime DataMetrica { get; set; }

    [Required]
    public TimeSpan HoraMetrica { get; set; }

    [Required]
    [StringLength(50)]
    public string TipoMetrica { get; set; } = string.Empty; // Diaria, Semanal, Mensal

    public int ChamadosAbertos { get; set; } = 0;
    public int ChamadosResolvidos { get; set; } = 0;
    public int ChamadosFechados { get; set; } = 0;
    public int ChamadosEscalados { get; set; } = 0;

    public decimal? TempoMedioResolucao { get; set; } // Em minutos
    public decimal? SatisfacaoMedia { get; set; } // 1.00 a 5.00

    public int UsuariosAtivos { get; set; } = 0;
    public int TecnicosAtivos { get; set; } = 0;

    public decimal? TaxaResolucaoPrimeiroContato { get; set; } // Percentual

    [StringLength(4000)]
    public string? ChamadosPorPrioridade { get; set; } // JSON

    [StringLength(4000)]
    public string? ChamadosPorCategoria { get; set; } // JSON

    [StringLength(4000)]
    public string? TempoResolucaoPorPrioridade { get; set; } // JSON

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Propriedades calculadas
    public string TipoMetricaDisplay => TipoMetrica switch
    {
        "Diaria" => "Diária",
        "Semanal" => "Semanal",
        "Mensal" => "Mensal",
        _ => TipoMetrica
    };

    public string DataMetricaDisplay => DataMetrica.ToString("dd/MM/yyyy");

    public string HoraMetricaDisplay => HoraMetrica.ToString(@"hh\:mm");

    public string TempoMedioResolucaoDisplay => TempoMedioResolucao.HasValue ? 
        $"{TempoMedioResolucao.Value:F1} min" : "N/A";

    public string SatisfacaoMediaDisplay => SatisfacaoMedia.HasValue ? 
        $"{SatisfacaoMedia.Value:F1}/5.0" : "N/A";

    public string TaxaResolucaoDisplay => TaxaResolucaoPrimeiroContato.HasValue ? 
        $"{TaxaResolucaoPrimeiroContato.Value:F1}%" : "N/A";

    public int TotalChamados => ChamadosAbertos + ChamadosResolvidos + ChamadosFechados;

    public decimal TaxaResolucao => TotalChamados > 0 ? 
        (decimal)ChamadosResolvidos / TotalChamados * 100 : 0;

    public decimal TaxaFechamento => TotalChamados > 0 ? 
        (decimal)ChamadosFechados / TotalChamados * 100 : 0;

    public Dictionary<string, int> ChamadosPorPrioridadeDict =>
        string.IsNullOrEmpty(ChamadosPorPrioridade) ? new Dictionary<string, int>() :
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(ChamadosPorPrioridade) ?? new Dictionary<string, int>();

    public Dictionary<string, int> ChamadosPorCategoriaDict =>
        string.IsNullOrEmpty(ChamadosPorCategoria) ? new Dictionary<string, int>() :
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(ChamadosPorCategoria) ?? new Dictionary<string, int>();

    public Dictionary<string, decimal> TempoResolucaoPorPrioridadeDict =>
        string.IsNullOrEmpty(TempoResolucaoPorPrioridade) ? new Dictionary<string, decimal>() :
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(TempoResolucaoPorPrioridade) ?? new Dictionary<string, decimal>();
}
