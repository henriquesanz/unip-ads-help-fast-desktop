using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class FAQItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Pergunta { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Resposta { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    // Propriedades calculadas para compatibilidade (não mapeadas no banco)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Titulo => Pergunta.Length > 200 ? Pergunta.Substring(0, 200) + "..." : Pergunta;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Descricao => Pergunta;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Solucao => Resposta;
    
    // Propriedades calculadas para compatibilidade (não mapeadas no banco)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Categoria => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Subcategoria => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Tags => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Utilidade => 0; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Visualizacoes => 0; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? DataCriacao => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? DataAtualizacao => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Usuario? CriadoPor => null; // Não existe no banco atual
}
