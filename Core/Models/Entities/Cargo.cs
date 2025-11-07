using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class Cargo
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    // Navegação
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

