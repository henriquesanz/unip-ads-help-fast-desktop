namespace HelpFastDesktop.Core.Models.Entities.JavaApi;

public class NotificacaoResponse
{
    public string? NotificacaoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
