using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class ConfiguracaoNotificacao
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public bool EmailAtivo { get; set; } = true;
    public bool PushAtivo { get; set; } = true;
    public bool InAppAtivo { get; set; } = true;

    // Configurações específicas
    public bool NotificarStatus { get; set; } = true;
    public bool NotificarAtribuicao { get; set; } = true;
    public bool NotificarResolucao { get; set; } = true;
    public bool NotificarComentarios { get; set; } = true;
    public bool NotificarEscalacao { get; set; } = true;

    [Required]
    [StringLength(20)]
    public string Frequencia { get; set; } = "Imediato"; // Imediato, Diario, Semanal

    public TimeSpan? HorarioSilenciosoInicio { get; set; }
    public TimeSpan? HorarioSilenciosoFim { get; set; }

    [StringLength(100)]
    public string? DiasSemana { get; set; } // JSON com dias da semana

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }

    // Propriedades calculadas
    public string FrequenciaDisplay => Frequencia switch
    {
        "Imediato" => "Imediato",
        "Diario" => "Diário",
        "Semanal" => "Semanal",
        _ => Frequencia
    };

    public bool TemHorarioSilencioso => HorarioSilenciosoInicio.HasValue && HorarioSilenciosoFim.HasValue;

    public string HorarioSilenciosoDisplay => TemHorarioSilencioso ? 
        $"{HorarioSilenciosoInicio.Value:hh\\:mm} - {HorarioSilenciosoFim.Value:hh\\:mm}" : 
        "Não configurado";

    public bool IsSilenciosoAgora
    {
        get
        {
            if (!TemHorarioSilencioso) return false;
            
            var agora = DateTime.Now.TimeOfDay;
            var inicio = HorarioSilenciosoInicio!.Value;
            var fim = HorarioSilenciosoFim!.Value;

            if (inicio <= fim)
                return agora >= inicio && agora <= fim;
            else
                return agora >= inicio || agora <= fim;
        }
    }

    public List<string> DiasSemanaList =>
        string.IsNullOrEmpty(DiasSemana) ? new List<string>() :
        System.Text.Json.JsonSerializer.Deserialize<List<string>>(DiasSemana) ?? new List<string>();

    public bool IsDiaAtivo(string diaSemana)
    {
        var dias = DiasSemanaList;
        return dias.Count == 0 || dias.Contains(diaSemana);
    }
}
