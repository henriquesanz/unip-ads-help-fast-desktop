using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Entities;

public class InteracaoIA
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(50)]
    public string TipoInteracao { get; set; } = string.Empty; // Chat, Categorizacao, Atribuicao

    [StringLength(1000)]
    public string? Pergunta { get; set; }

    [StringLength(2000)]
    public string? Resposta { get; set; }

    [StringLength(100)]
    public string? Categoria { get; set; }

    public bool? ProblemaResolvido { get; set; }

    public int? Satisfacao { get; set; } // 1-5

    public int? TempoResposta { get; set; } // Em segundos

    public decimal? Confianca { get; set; } // 0.00 a 1.00

    public DateTime DataInteracao { get; set; } = DateTime.Now;

    public int? ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    // Propriedades calculadas
    public string TipoInteracaoDisplay => TipoInteracao switch
    {
        "Chat" => "Chat",
        "Categorizacao" => "Categorização",
        "Atribuicao" => "Atribuição",
        _ => TipoInteracao
    };

    public string SatisfacaoDisplay => Satisfacao.HasValue ? $"{Satisfacao.Value}/5" : "Não avaliado";

    public string ConfiancaDisplay => Confianca.HasValue ? $"{Confianca.Value:P0}" : "Não informado";

    public string TempoRespostaDisplay => TempoResposta.HasValue ? $"{TempoResposta.Value}s" : "Não informado";

    public bool IsResolvido => ProblemaResolvido == true;
    public bool IsBemAvaliado => Satisfacao.HasValue && Satisfacao.Value >= 4;
    public bool IsAltaConfianca => Confianca.HasValue && Confianca.Value >= 0.8m;
}
