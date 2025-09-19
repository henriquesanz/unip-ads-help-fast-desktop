using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Models;

public class Notificacao
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Mensagem { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Prioridade { get; set; } = "Media"; // Baixa, Media, Alta, Urgente

    public bool Lida { get; set; } = false;

    public DateTime DataEnvio { get; set; } = DateTime.Now;

    public DateTime? DataLeitura { get; set; }

    [Required]
    [StringLength(50)]
    public string Canal { get; set; } = "InApp"; // InApp, Email, Push, SMS

    public int? ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    [StringLength(100)]
    public string? Acao { get; set; }

    // Propriedades calculadas
    public string PrioridadeDisplay => Prioridade switch
    {
        "Baixa" => "Baixa",
        "Media" => "Média",
        "Alta" => "Alta",
        "Urgente" => "Urgente",
        _ => Prioridade
    };

    public string CanalDisplay => Canal switch
    {
        "InApp" => "No Sistema",
        "Email" => "E-mail",
        "Push" => "Push",
        "SMS" => "SMS",
        _ => Canal
    };

    public bool IsUrgente => Prioridade == "Urgente";
    public bool IsAlta => Prioridade == "Alta";
    public TimeSpan? TempoParaLeitura => DataLeitura.HasValue ? DataLeitura.Value - DataEnvio : null;
}
