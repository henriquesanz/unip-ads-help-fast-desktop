namespace HelpFastDesktop.Models.JavaApi;

public class ChatResponse
{
    public string Resposta { get; set; } = string.Empty;
    public bool EscalarParaHumano { get; set; } = false;
    public string? Categoria { get; set; }
    public decimal? Confianca { get; set; }
    public List<string>? SugestoesFAQ { get; set; }
}
