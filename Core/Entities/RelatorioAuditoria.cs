namespace HelpFastDesktop.Core.Entities;

public class RelatorioAuditoria
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int TotalAcoes { get; set; }
    public int UsuariosAtivos { get; set; }
    public Dictionary<string, int> AcoesPorTipo { get; set; } = new();
    public Dictionary<string, int> AcoesPorTabela { get; set; } = new();
    public Dictionary<string, int> AcoesPorUsuario { get; set; } = new();
    public List<LogAuditoria> LogsCriticos { get; set; } = new();
    public List<AtividadeUsuario> AtividadeUsuarios { get; set; } = new();
}

public class AtividadeUsuario
{
    public int? UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int TotalAcoes { get; set; }
    public DateTime PrimeiraAcao { get; set; }
    public DateTime UltimaAcao { get; set; }
    public List<string> TiposAcoes { get; set; } = new();
    public List<string> TabelasAcessadas { get; set; } = new();
}
