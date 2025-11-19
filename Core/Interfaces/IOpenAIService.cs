using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HelpFastDesktop.Core.Interfaces;

public interface IOpenAIService
{
    Task<string> EnviarPerguntaAsync(string perguntaUsuario, string systemPrompt, CancellationToken cancellationToken = default);
    Task<string> EnviarPerguntaComHistoricoAsync(string perguntaUsuario, string systemPrompt, List<ChatMessage> historico, CancellationToken cancellationToken = default);
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "system", "user", "assistant"
    public string Content { get; set; } = string.Empty;
}

