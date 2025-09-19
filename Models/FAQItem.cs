using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Models;

public class FAQItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Solucao { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Categoria { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subcategoria { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; } // Separadas por vírgula

    public int Visualizacoes { get; set; } = 0;

    public int? Utilidade { get; set; } // Rating 1-5

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public DateTime? DataAtualizacao { get; set; }

    [Required]
    public int CriadoPorId { get; set; }
    public Usuario? CriadoPor { get; set; }

    // Propriedades calculadas
    public List<string> TagsList => 
        string.IsNullOrEmpty(Tags) ? new List<string>() : 
        Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

    public string UtilidadeDisplay => Utilidade.HasValue ? $"{Utilidade.Value}/5" : "Não avaliado";

    public string StatusDisplay => Ativo ? "Ativo" : "Inativo";

    public bool IsPopular => Visualizacoes > 100;
    public bool IsBemAvaliado => Utilidade.HasValue && Utilidade.Value >= 4;
}
