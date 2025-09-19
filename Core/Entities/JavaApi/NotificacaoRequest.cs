namespace HelpFastDesktop.Core.Entities.JavaApi;

public class NotificacaoRequest
{
    public string Tipo { get; set; } = string.Empty;
    public Destinatario Destinatario { get; set; } = new();
    public ChamadoInfo? Chamado { get; set; }
    public TecnicoInfo? Tecnico { get; set; }
}

public class Destinatario
{
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}

public class ChamadoInfo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Prioridade { get; set; } = string.Empty;
}

public class TecnicoInfo
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
