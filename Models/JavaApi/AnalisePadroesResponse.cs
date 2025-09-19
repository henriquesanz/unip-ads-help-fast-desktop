namespace HelpFastDesktop.Models.JavaApi;

public class AnalisePadroesResponse
{
    public List<TendenciaCategoria>? TendenciaCategorias { get; set; }
    public List<TendenciaTempo>? TendenciaTempo { get; set; }
    public List<ProblemaRecorrente>? ProblemasRecorrentes { get; set; }
    public EstatisticasGerais? Estatisticas { get; set; }
}

public class TendenciaCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public double PercentualVariacao { get; set; }
}

public class TendenciaTempo
{
    public DateTime Data { get; set; }
    public int QuantidadeChamados { get; set; }
    public double TempoMedioResolucao { get; set; }
}

public class ProblemaRecorrente
{
    public string Descricao { get; set; } = string.Empty;
    public int Frequencia { get; set; }
    public string? Categoria { get; set; }
}

public class EstatisticasGerais
{
    public int TotalChamados { get; set; }
    public double TempoMedioResolucao { get; set; }
    public double TaxaResolucao { get; set; }
    public string? CategoriaMaisComum { get; set; }
}
