namespace HelpFastDesktop.Core.Models.Entities;

public class HistoricoNotificacoes
{
    public int Id { get; set; }
    public int NotificacaoId { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }
    
    // Navigation properties
    public Notificacao? Notificacao { get; set; }
    public Usuario? Usuario { get; set; }
}
