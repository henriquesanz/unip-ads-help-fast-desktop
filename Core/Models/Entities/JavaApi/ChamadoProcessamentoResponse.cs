namespace HelpFastDesktop.Core.Models.Entities.JavaApi;

public class ChamadoProcessamentoResponse
{
    public int ChamadoId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Subcategoria { get; set; } = string.Empty;
    public int? TecnicoId { get; set; }
    public string? TecnicoNome { get; set; }
    public string? TecnicoEmail { get; set; }
    public decimal Confianca { get; set; }
    public List<string> Sugestoes { get; set; } = new();
    public bool NotificacaoEnviada { get; set; }
    public DateTime Timestamp { get; set; }
}
