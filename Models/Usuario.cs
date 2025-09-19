using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public UserRole TipoUsuario { get; set; } = UserRole.Cliente;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public DateTime? UltimoLogin { get; set; }

    public bool Ativo { get; set; } = true;

    // Propriedades de navegação
    public int? CriadoPorId { get; set; }
    public Usuario? CriadoPor { get; set; }

    // Propriedades calculadas
    public string TipoUsuarioDisplay => TipoUsuario.GetDisplayName();
    public string TipoUsuarioDescription => TipoUsuario.GetDescription();
    public bool PodeGerenciarUsuarios => TipoUsuario.CanManageUsers();
    public bool PodeVerTodosChamados => TipoUsuario.CanViewAllTickets();
}

