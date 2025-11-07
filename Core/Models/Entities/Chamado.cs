using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class Chamado
{
    public int Id { get; set; }

    [Required]
    [StringLength(2000)]
    public string Motivo { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Usuario? Cliente { get; set; }

    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Aberto";

    public DateTime DataAbertura { get; set; } = DateTime.Now;

    public DateTime? DataFechamento { get; set; }

    // Propriedades calculadas para compatibilidade (não mapeadas no banco)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Titulo => Motivo.Length > 100 ? Motivo.Substring(0, 100) + "..." : Motivo;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Descricao => Motivo;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime DataCriacao => DataAbertura;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? DataResolucao => DataFechamento;

    // Propriedades calculadas
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string StatusDisplay => Status switch
    {
        "Aberto" => "Aberto",
        "EmAndamento" => "Em Andamento",
        "Resolvido" => "Resolvido",
        "Fechado" => "Fechado",
        _ => Status
    };

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool PodeSerEditado => Status == "Aberto" || Status == "EmAndamento";
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EstaResolvido => Status == "Resolvido" || Status == "Fechado";
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public TimeSpan TempoDecorrido => DateTime.Now - DataAbertura;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public TimeSpan? TempoTotalResolucao => DataFechamento.HasValue ? DataFechamento.Value - DataAbertura : null;
    
    // Propriedades calculadas para compatibilidade (não mapeadas no banco)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int UsuarioId => ClienteId;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Usuario? Usuario => Cliente;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Prioridade => "Media"; // Valor padrão, pode ser ajustado conforme necessário
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsUrgente => false; // Valor padrão
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Categoria => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public TimeSpan? TempoResolucao => DataFechamento.HasValue ? DataFechamento.Value - DataAbertura : null;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? Satisfacao => null; // Não existe no banco atual
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsAtrasado => false; // Pode ser calculado baseado em regras de negócio
}
