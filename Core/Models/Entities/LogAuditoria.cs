using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class LogAuditoria
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(100)]
    public string Acao { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Tabela { get; set; } = string.Empty;

    public int? RegistroId { get; set; }

    [StringLength(4000)]
    public string? DadosAntigos { get; set; } // JSON

    [StringLength(4000)]
    public string? DadosNovos { get; set; } // JSON

    [StringLength(45)]
    public string? IPAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime DataAcao { get; set; } = DateTime.Now;

    // Propriedades calculadas
    public string TipoOperacao => Acao switch
    {
        "INSERT" => "Inserção",
        "UPDATE" => "Atualização",
        "DELETE" => "Exclusão",
        "SELECT" => "Consulta",
        "LOGIN" => "Login",
        "LOGOUT" => "Logout",
        _ => Acao
    };

    public bool IsOperacaoCritica => Acao == "DELETE" || Acao == "UPDATE";

    public string ResumoOperacao => $"{TipoOperacao} em {Tabela}" + 
        (RegistroId.HasValue ? $" (ID: {RegistroId.Value})" : "");

    public string UsuarioDisplay => Usuario?.Nome ?? "Sistema";
}
