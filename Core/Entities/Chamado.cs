using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Entities;

public class Chamado
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Aberto";

    [Required]
    public string Prioridade { get; set; } = "Média";

    [StringLength(100)]
    public string? Categoria { get; set; } // Categoria automática da IA

    [StringLength(100)]
    public string? Subcategoria { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public DateTime? DataAtualizacao { get; set; }

    public DateTime? DataResolucao { get; set; }

    public DateTime? DataFechamento { get; set; }

    // Métricas e avaliações
    public int? TempoResolucao { get; set; } // Em minutos
    public int? Satisfacao { get; set; } // 1-5 estrelas
    [StringLength(500)]
    public string? ComentarioSatisfacao { get; set; }

    // Relacionamentos
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    // Propriedades calculadas
    public string StatusDisplay => Status switch
    {
        "Aberto" => "Aberto",
        "EmAndamento" => "Em Andamento",
        "Resolvido" => "Resolvido",
        "Fechado" => "Fechado",
        _ => Status
    };

    public string PrioridadeDisplay => Prioridade switch
    {
        "Baixa" => "Baixa",
        "Media" => "Média",
        "Alta" => "Alta",
        "Critica" => "Crítica",
        _ => Prioridade
    };

    public bool PodeSerEditado => Status == "Aberto" || Status == "EmAndamento";
    public bool EstaResolvido => Status == "Resolvido" || Status == "Fechado";

    // Propriedades adicionais
    public string SatisfacaoDisplay => Satisfacao.HasValue ? $"{Satisfacao.Value}/5" : "Não avaliado";
    public string TempoResolucaoDisplay => TempoResolucao.HasValue ? $"{TempoResolucao.Value} min" : "N/A";
    public string CategoriaDisplay => Categoria ?? "Não categorizado";
    public string SubcategoriaDisplay => Subcategoria ?? "Não definida";
    
    public TimeSpan TempoDecorrido => DateTime.Now - DataCriacao;
    public TimeSpan? TempoTotalResolucao => DataResolucao.HasValue ? DataResolucao.Value - DataCriacao : null;
    
    public bool IsUrgente => Prioridade == "Critica" || Prioridade == "Alta";
    public bool IsAtrasado => DataCriacao.AddDays(GetSLAEmDias()) < DateTime.Now && !EstaResolvido;
    
    private int GetSLAEmDias() => Prioridade switch
    {
        "Critica" => 1,
        "Alta" => 3,
        "Media" => 7,
        "Baixa" => 14,
        _ => 7
    };
}

