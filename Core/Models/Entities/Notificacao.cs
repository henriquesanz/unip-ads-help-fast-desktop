using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

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
    public string Prioridade { get; set; } = "Media";

    public bool Lida { get; set; } = false;

    public DateTime DataEnvio { get; set; } = DateTime.Now;

    public DateTime? DataLeitura { get; set; }

    public int? ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    [StringLength(100)]
    public string? Acao { get; set; }

    public string PrioridadeDisplay => Prioridade switch
    {
        "Baixa" => "Baixa",
        "Media" => "Média",
        "Alta" => "Alta",
        "Urgente" => "Urgente",
        _ => Prioridade
    };

    public bool IsUrgente => Prioridade == "Urgente";
    public bool IsAlta => Prioridade == "Alta";
    public TimeSpan? TempoParaLeitura => DataLeitura.HasValue ? DataLeitura.Value - DataEnvio : null;
}
