namespace HelpFastDesktop.Core.Entities.JavaApi;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dependencias Dependencias { get; set; } = new();
    public string Versao { get; set; } = string.Empty;
}

public class Dependencias
{
    public ServicoStatus OpenAI { get; set; } = new();
    public ServicoStatus Email { get; set; } = new();
    public ServicoStatus Database { get; set; } = new();
}

public class ServicoStatus
{
    public string Status { get; set; } = string.Empty;
    public string Latencia { get; set; } = string.Empty;
}
