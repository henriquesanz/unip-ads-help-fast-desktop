using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class Comentario
{
    public int Id { get; set; }

    [Required]
    public int ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(2000)]
    public string ComentarioTexto { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = "Publico"; // Publico, Interno, Sistema

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public bool VisivelCliente { get; set; } = true;

    // Propriedades calculadas
    public string TipoDisplay => Tipo switch
    {
        "Publico" => "Público",
        "Interno" => "Interno",
        "Sistema" => "Sistema",
        _ => Tipo
    };

    public bool IsInterno => Tipo == "Interno";
    public bool IsSistema => Tipo == "Sistema";
    public bool IsPublico => Tipo == "Publico";
}
