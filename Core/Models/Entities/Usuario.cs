using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

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
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    public int CargoId { get; set; }
    public Cargo? Cargo { get; set; }

    [StringLength(15)]
    public string? Telefone { get; set; }

    public DateTime? UltimoLogin { get; set; }

    // Propriedades calculadas (baseadas no Cargo) - não mapeadas no banco
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string TipoUsuarioDisplay => Cargo?.Nome ?? "Sem cargo";
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public UserRole TipoUsuario => GetUserRoleFromCargo();
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string TipoUsuarioDescription => TipoUsuario.GetDescription();
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool PodeGerenciarUsuarios => TipoUsuario.CanManageUsers();
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool PodeVerTodosChamados => TipoUsuario.CanViewAllTickets();

    private UserRole GetUserRoleFromCargo()
    {
        if (Cargo == null) return UserRole.Cliente;
        
        return Cargo.Nome.ToLower() switch
        {
            "administrador" => UserRole.Administrador,
            "técnico" or "tecnico" => UserRole.Tecnico,
            _ => UserRole.Cliente
        };
    }
}
