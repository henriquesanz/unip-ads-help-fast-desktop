using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Entities;

public class Relatorio
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty; // Performance, Volume, Satisfacao, Customizado

    [StringLength(500)]
    public string? Descricao { get; set; }

    [StringLength(4000)]
    public string? Parametros { get; set; } // JSON com parâmetros do relatório

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    [Required]
    public int CriadoPorId { get; set; }
    public Usuario? CriadoPor { get; set; }

    public bool Agendado { get; set; } = false;

    [StringLength(20)]
    public string? Frequencia { get; set; } // Diario, Semanal, Mensal

    public DateTime? ProximaExecucao { get; set; }

    public DateTime? UltimaExecucao { get; set; }

    [StringLength(4000)]
    public string? Destinatarios { get; set; } // JSON com lista de emails

    [Required]
    [StringLength(20)]
    public string Formato { get; set; } = "PDF"; // PDF, Excel, CSV

    public bool Ativo { get; set; } = true;

    // Propriedades calculadas
    public string TipoDisplay => Tipo switch
    {
        "Performance" => "Performance",
        "Volume" => "Volume",
        "Satisfacao" => "Satisfação",
        "Customizado" => "Customizado",
        _ => Tipo
    };

    public string FrequenciaDisplay => Frequencia switch
    {
        "Diario" => "Diário",
        "Semanal" => "Semanal",
        "Mensal" => "Mensal",
        _ => Frequencia ?? "Não agendado"
    };

    public string FormatoDisplay => Formato switch
    {
        "PDF" => "PDF",
        "Excel" => "Excel",
        "CSV" => "CSV",
        _ => Formato
    };

    public string StatusDisplay => Agendado ? "Agendado" : "Manual";

    public TimeSpan Duracao => DataFim - DataInicio;

    public string PeriodoDisplay => $"{DataInicio:dd/MM/yyyy} - {DataFim:dd/MM/yyyy}";

    public bool IsExecutando => ProximaExecucao.HasValue && ProximaExecucao.Value <= DateTime.Now;

    public List<string> DestinatariosList =>
        string.IsNullOrEmpty(Destinatarios) ? new List<string>() :
        System.Text.Json.JsonSerializer.Deserialize<List<string>>(Destinatarios) ?? new List<string>();

    public Dictionary<string, object> ParametrosDict =>
        string.IsNullOrEmpty(Parametros) ? new Dictionary<string, object>() :
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Parametros) ?? new Dictionary<string, object>();
}
