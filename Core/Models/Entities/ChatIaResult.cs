using System.ComponentModel.DataAnnotations;

namespace HelpFastDesktop.Core.Models.Entities;

public class ChatIaResult
{
    public int Id { get; set; }

    public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    [Required]
    public string ResultJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

