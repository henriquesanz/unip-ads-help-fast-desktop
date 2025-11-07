using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class Chat
{
    public int Id { get; set; }

    public int? ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    public int? ChamadoId1 { get; set; }
    public Chamado? Chamado1 { get; set; }

    public int RemetenteId { get; set; }
    public Usuario? Remetente { get; set; }

    public int? DestinatarioId { get; set; }
    public Usuario? Destinatario { get; set; }

    [Required]
    [StringLength(4000)]
    public string Mensagem { get; set; } = string.Empty;

    public DateTime DataEnvio { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? Tipo { get; set; }
}

