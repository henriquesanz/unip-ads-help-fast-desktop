using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Entities;

public class ConfiguracaoSistema
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Chave { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Valor { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = "String"; // String, Int, Boolean, JSON

    [StringLength(200)]
    public string? Descricao { get; set; }

    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    [Required]
    public int AtualizadoPorId { get; set; }
    public Usuario? AtualizadoPor { get; set; }

    // Propriedades calculadas
    public string TipoDisplay => Tipo switch
    {
        "String" => "Texto",
        "Int" => "Número Inteiro",
        "Boolean" => "Verdadeiro/Falso",
        "JSON" => "JSON",
        _ => Tipo
    };

    public object ValorConvertido => Tipo switch
    {
        "Int" => int.TryParse(Valor, out var intVal) ? intVal : 0,
        "Boolean" => bool.TryParse(Valor, out var boolVal) && boolVal,
        "JSON" => System.Text.Json.JsonSerializer.Deserialize<object>(Valor) ?? Valor,
        _ => Valor
    };

    public bool IsConfiguracaoCritica => Chave.Contains("api") || Chave.Contains("url") || Chave.Contains("senha");

    public bool IsConfiguracaoSistema => Chave.StartsWith("sistema_");
    public bool IsConfiguracaoAPI => Chave.StartsWith("api_");
    public bool IsConfiguracaoEmail => Chave.StartsWith("email_");
    public bool IsConfiguracaoIA => Chave.StartsWith("ia_");
}
