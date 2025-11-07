using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class HistoricoChamado
{
    public int Id { get; set; }

    public int ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    [Required]
    [StringLength(200)]
    public string Acao { get; set; } = string.Empty;

    public DateTime Data { get; set; } = DateTime.Now;

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}

